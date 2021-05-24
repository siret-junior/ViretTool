using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Viret.DataModel
{
    /// <summary>
    /// Holds a tree structure of the input video dataset.
    /// 
    /// Simple tree structure description:
    /// The videos are split into sequential shots from which were selected its representative frames.
    /// Dataset 
    ///     -> Videos (sequentially ordered) 
    ///     -> Shots (sequentially ordered)
    ///     -> Frames (sequentially ordered)
    /// 
    /// Extended tree structure explanation (used in the extraction phase):
    /// The representative frames are a subset of a set of playback frames (k-FramesPerSecond).
    /// The playback frames are a subset of all frames from a video.
    /// Dataset 
    ///     -> Videos 
    ///     -> Shots
    ///     -> (Representative) Frames (of shots) 
    ///     -[subset]-> Playback Frames (k-FPS)
    ///     -[subset]-> All video file frames (variable FPS per video)
    /// </summary>
    public class Dataset
    {
        // hierarchy
        public readonly List<Video> Videos = new List<Video>();
        public readonly List<Shot> Shots = new List<Shot>();
        public readonly List<Keyframe> Keyframes = new List<Keyframe>();


        public Dataset(string inputFile)
        {
            // parse records
            var records = File.ReadAllLines(inputFile)
                .AsParallel()
                .Select((relativePath, index) =>
                {
                    ParseFrameHierarchy(Path.GetFileName(relativePath),
                        out int videoId,
                        out int shotId,
                        out int shotStartFrame,
                        out int shotEndFrame,
                        out int frameNumber,
                        out _);
                    return new
                    {
                        Id = index,
                        VideoId = videoId,
                        ShotId = shotId,
                        ShotStartFrame = shotStartFrame,
                        ShotEndFrame = shotEndFrame,
                        FrameNumber = frameNumber
                    };
                });

            // build a tree structure
            foreach (var record in records.OrderBy(record => record.Id))
            {
                Video video = GetOrAppendVideo(this, record.VideoId);
                Shot shot = GetOrAppendShot(video, record.ShotId, record.ShotStartFrame, record.ShotEndFrame);
                AppendKeyframe(shot, record.FrameNumber);
            }
        }


        public static Dataset FromDirectory(string inputDirectory, string extension = ".dataset")
        {
            string inputFile = Directory.GetFiles(inputDirectory, $"*{extension}").FirstOrDefault();
            if (inputFile == null)
            {
                throw new FileNotFoundException($"Dataset file was not found in directory '{inputDirectory}'");
            }

            return new Dataset(inputFile);
        }


        private static readonly Regex _frameHierarchyFormatRegex = new Regex(
            @"^[Vv](?<videoId>[0-9]+)"
            + @"_"
            + @"[Ss](?<shotId>[0-9]+)"
                + @"\("
                + @"[Ff](?<shotStartFrame>[0-9]+)"
                + @"-"
                + @"[Ff](?<shotEndFrame>[0-9]+)"
                + @"\)"
            + @"_"
            + @"[Ff](?<frameNumber>[0-9]+)"
            + @"\.(?<extension>.*)$",
            RegexOptions.ExplicitCapture);


        /// <summary>
        /// Parses the frame heirarchy using a regular expression.
        /// </summary>
        /// <param name="inputString">Input string storing the dataset heirarchy.</param>
        /// <param name="videoId">Parsed videoId.</param>
        /// <param name="shotId">Parsed shotId.</param>
        /// <param name="shotStartFrame">Parsed starting frame of the shot.</param>
        /// <param name="shotEndFrame">Parsed ending frame of the shot (including this frame).</param>
        /// <param name="frameNumber">Frame number in the original video.</param>
        /// <param name="extension">Extension of the file in the filelist.</param>
        private static void ParseFrameHierarchy(string inputString,
            out int videoId,
            out int shotId,
            out int shotStartFrame,
            out int shotEndFrame,
            out int frameNumber,
            out string extension)
        {
            Match match = _frameHierarchyFormatRegex.Match(inputString);
            if (!match.Success)
            {
                throw new ArgumentException("Error parsing frame heirarchy: " + inputString);
            }

            videoId = int.Parse(match.Groups["videoId"].Value);
            shotId = int.Parse(match.Groups["shotId"].Value);
            shotStartFrame = int.Parse(match.Groups["shotStartFrame"].Value);
            shotEndFrame = int.Parse(match.Groups["shotEndFrame"].Value);
            frameNumber = int.Parse(match.Groups["frameNumber"].Value);
            extension = match.Groups["extension"].Value;

            if (frameNumber < shotStartFrame || frameNumber > shotEndFrame)
            {
                throw new ArgumentException($"Frame number {frameNumber} is not in shot range {shotStartFrame}-{shotEndFrame}.");
            }
        }


        /// <summary>
        /// If the videoId matches the last added Video then it returns it.
        /// Otherwise, if the videoId increments sequentially, then it creates a new Video instance.
        /// Throws an exception if the videoId is out of range or it does not increment sequentially.
        /// </summary>
        /// <param name="parentDataset">Parent dataset.</param>
        /// <param name="videoId">ID of the video.</param>
        /// <returns>An existing or newly created Video instance matching the input videoId.</returns>
        private static Video GetOrAppendVideo(Dataset parentDataset, int videoId)
        {
            if (videoId < 0)
            {
                throw new ArgumentOutOfRangeException($"Video ID can not be negative: {videoId}.");
            }

            // get or append video
            List<Video> videos = parentDataset.Videos;
            Video video;
            if (videos.Count == 0)
            {
                // add the first video
                video = new Video(parentDataset, videoId);
            }
            else if (videos[videos.Count - 1].Id == videoId)
            {
                // get the last video
                video = videos[videos.Count - 1];
            }
            else if (videos[videos.Count - 1].Id == videoId - 1)
            {
                // append a new video
                video = new Video(parentDataset, videoId);
            }
            else
            {
                int lastVideoId = videos[videos.Count - 1].Id;
                throw new ArgumentException($"Input video IDs do not increment sequentially (last: {lastVideoId}, current: {videoId}).");
            }

            return video;
        }

        /// <summary>
        /// If the shotId matches the last added Shot then it returns it.
        /// Otherwise, if the shotId increments sequentially, then it creates a new Shot instance.
        /// Throws an exception if the shotId is out of range or it does not increment sequentially.
        /// </summary>
        /// <param name="parentVideo">Parent Video to append the shot to.</param>
        /// <param name="shotId">ID of the shot.</param>
        /// <param name="shotStartFrame">Start frame number of the shot in the original video.</param>
        /// <param name="shotEndFrame">End frame number of the shot in the original video.</param>
        /// <returns></returns>
        private static Shot GetOrAppendShot(Video parentVideo, int shotId, int shotStartFrame, int shotEndFrame)
        {
            if (shotId < 0)
            {
                throw new ArgumentOutOfRangeException($"ShotId can not be negative: {shotId}.");
            }

            // get or append shot
            List<Shot> shots = parentVideo.Shots;
            Shot shot;
            if (shots.Count == 0)
            {
                // add the first shot
                shot = new Shot(parentVideo, shotId, shotStartFrame, shotEndFrame);
            }
            else if (shots[shots.Count - 1].IdInVideo == shotId)
            {
                // get the last shot
                shot = shots[shots.Count - 1];
            }
            else if (shots[shots.Count - 1].IdInVideo == shotId - 1)
            {
                // append a new shot
                shot = new Shot(parentVideo, shotId, shotStartFrame, shotEndFrame);
            }
            else
            {
                // fill missing values with empty shots
                int lastShotId = shots[shots.Count - 1].Id;
                int nTries = 0;
                int maxTries = 100;
                while (shots[shots.Count - 1].IdInVideo != shotId - 1 && nTries++ < maxTries)
                {
                    new Shot(parentVideo, shots[shots.Count - 1].IdInVideo + 1, -1, -1);
                }
                // append the proper shot
                shot = new Shot(parentVideo, shotId, shotStartFrame, shotEndFrame);

                if (nTries == maxTries)
                {
                    throw new ArgumentException($"Input shot IDs do not increment sequentially and adding {maxTries} empty shots did not help (last: {lastShotId}, current: {shotId}).");
                }
            }

            return shot;
        }


        /// <summary>
        /// Creates Frame instance and links it to its parent video and shot.
        /// </summary>
        /// <param name="video">Parent video instance.</param>
        /// <param name="parentShot">Parent shot instance.</param>
        /// <param name="frameInVideo">Frame number in the original video.</param>
        /// <returns>Newly created Frame instance.</returns>
        private static Keyframe AppendKeyframe(Shot parentShot, int frameInVideo)
        {
            if (frameInVideo < 0)
            {
                throw new ArgumentOutOfRangeException($"Frame in video can not be negative: {frameInVideo}.");
            }

            // create keyframe instance
            Video parentVideo = parentShot.ParentVideo;
            Keyframe keyframe = new Keyframe(parentVideo, parentVideo.Keyframes.Count, frameInVideo, parentShot, parentShot.Keyframes.Count);

            // link to parents
            parentShot.Keyframes.Add(keyframe);
            parentShot.ParentVideo.Keyframes.Add(keyframe);
            
            return keyframe;
        }
    }
}
