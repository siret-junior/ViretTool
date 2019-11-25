using System;
using System.Collections.Generic;
using System.IO;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    /// <summary>
    /// Reads dataset structure (video -> shot -> frame) from a filelist.
    /// The heirarchy is encoded in the filelist filenames.
    /// Each filename corresponds to a single frame.
    /// Each filename specifies video, shot, shot boundaries and frame number of the frame.
    /// FrameId corresponds to the filelist line number starting from 0.
    /// </summary>    
    public class DatasetReader
    {
        // entities
        private List<Video> _videos;
        private List<Shot> _shots;
        private List<Frame> _frames;

        // mappings
        private List<List<Shot>> _videoShotMappings;
        private List<List<Frame>> _videoFrameMappings;
        private List<List<Frame>> _shotFrameMappings;


        public virtual Dataset ReadDataset(StreamReader reader)
        {
            ResetEntitiesAndMappings();

            while (!reader.EndOfStream)
            {
                string relativePath = reader.ReadLine();
                string filename = Path.GetFileName(relativePath);

                ParseFrameHeirarchy(filename,
                    out int videoId,
                    out int shotId,
                    out int shotStartFrame,
                    out int shotEndFrame,
                    out int frameNumber,
                    out string extension);

                Video video = GetOrAppendVideo(videoId);
                Shot shot = GetOrAppendShot(video, shotId, shotStartFrame, shotEndFrame);
                AppendFrame(video, shot, frameNumber);
            }

            // set mappings
            SetVideoShotMappings();
            SetVideoFrameMappings();
            SetShotFrameMappings();

            // construct dataset
            Dataset dataset = new Dataset(_videos.ToArray(), _shots.ToArray(), _frames.ToArray());
            return dataset;
        }

        public virtual Dataset ReadDataset(string inputFile)
        {
            using (StreamReader reader = new StreamReader(inputFile))
            {
                return ReadDataset(reader);
            }
        }


        /// <summary>
        /// If the videoId matches the last added Video then it returns it.
        /// Otherwise, if the videoId increments sequentially, then it creates a new Video instance.
        /// Throws an exception if the videoId is out of range or it does not increment sequentially.
        /// </summary>
        /// <param name="videoId">ID of the video.</param>
        /// <returns>An existing or newly created Video instance matching the input videoId.</returns>
        private Video GetOrAppendVideo(int videoId)
        {
            if (videoId < 0)
            {
                throw new ArgumentOutOfRangeException($"VideoId can not be negative: {videoId}.");
            }

            int lastVideoId = _videos.Count - 1;
            if (lastVideoId == videoId)
            {
                // this is the last added video
                return _videos[videoId];
            }
            else if (lastVideoId + 1 == videoId)
            {
                // this is a newly added video
                Video video = new Video(videoId);
                _videos.Add(video);

                // initialize children mapping lists
                _videoShotMappings.Add(new List<Shot>());
                _videoFrameMappings.Add(new List<Frame>());

                return video;
            }
            else
            {
                throw new ArgumentException($"Input video IDs do not increment sequentially (last: {lastVideoId}, current: {videoId}).");
            }
        }

        /// <summary>
        /// If the shotId matches the last added Shot then it returns it.
        /// Otherwise, if the shotId increments sequentially, then it creates a new Shot instance.
        /// Throws an exception if the shotId is out of range or it does not increment sequentially.
        /// </summary>
        /// <param name="video">Parent Video to append the shot to.</param>
        /// <param name="shotId">ID of the shot.</param>
        /// <param name="shotStartFrame">Start frame number of the shot in the original video.</param>
        /// <param name="shotEndFrame">End frame number of the shot in the original video.</param>
        /// <returns></returns>
        private Shot GetOrAppendShot(Video video, int shotId, int shotStartFrame, int shotEndFrame)
        {
            if (shotId < 0)
            {
                throw new ArgumentOutOfRangeException($"ShotId can not be negative: {shotId}.");
            }

            // load shots of the parent video
            List<Shot> videoShots = _videoShotMappings[video.Id];

            int lastShotId = videoShots.Count - 1;
            if (lastShotId == shotId)
            {
                // this is the last added shot
                return videoShots[shotId];
            }
            else if (lastShotId + 1 == shotId)
            {
                // this is a newly added shot
                Shot shot = new Shot(_shots.Count, shotId, shotStartFrame, shotEndFrame);
                _shots.Add(shot);

                // append to parent
                videoShots.Add(shot);

                // initialize children mappings
                _shotFrameMappings.Add(new List<Frame>());

                return shot;
            }
            else
            {
                throw new ArgumentException($"Input shot IDs do not increment sequentially (last: {lastShotId}, current: {shotId}).");
            }
        }

        /// <summary>
        /// Creates Frame instance and links it to its parent video and shot.
        /// </summary>
        /// <param name="video">Parent video instance.</param>
        /// <param name="shot">Parent shot instance.</param>
        /// <param name="frameNumber">Frame number in the original video.</param>
        /// <returns>Newly created Frame instance.</returns>
        private Frame AppendFrame(Video video, Shot shot, int frameNumber)
        {
            if (frameNumber < 0)
            {
                throw new ArgumentOutOfRangeException($"FrameNumber can not be negative: {frameNumber}.");
            }

            // create frame instance
            int frameId = _frames.Count;
            Frame frame = new Frame(frameId, frameNumber);

            // add to global list of frames
            _frames.Add(frame);

            // add parent mappings
            _videoFrameMappings[video.Id].Add(frame);
            _shotFrameMappings[shot.Id].Add(frame);

            return frame;
        }


        /// <summary>
        /// Assigns locally stored video->shot mappings to each Video instance.
        /// </summary>
        private void SetVideoShotMappings()
        {
            foreach (Video video in _videos)
            {
                Shot[] shotMappings = _videoShotMappings[video.Id].ToArray();
                video.SetShotMappings(shotMappings);
            }
        }


        /// <summary>
        /// Assigns locally stored video->frame mappings to each Video instance.
        /// </summary>
        private void SetVideoFrameMappings()
        {
            foreach (Video video in _videos)
            {
                Frame[] frameMappings = _videoFrameMappings[video.Id].ToArray();
                video.SetFrameMappings(frameMappings);
            }
        }

        /// <summary>
        /// Assigns locally stored shot->frame mappings to each Shot instance.
        /// </summary>
        private void SetShotFrameMappings()
        {
            foreach (Shot shot in _shots)
            {
                Frame[] frameMappings = _shotFrameMappings[shot.Id].ToArray();
                shot.SetFrameMappings(frameMappings);
            }
        }


        /// <summary>
        /// Initializes (empties) entity and mapping lists.
        /// </summary>
        private void ResetEntitiesAndMappings()
        {
            // entities
            _videos = new List<Video>();
            _shots = new List<Shot>();
            _frames = new List<Frame>();

            // mappings
            _videoShotMappings = new List<List<Shot>>();
            _videoFrameMappings = new List<List<Frame>>();
            _shotFrameMappings = new List<List<Frame>>();
        }


        private static readonly System.Text.RegularExpressions.Regex _tokenFormatRegex
            = new System.Text.RegularExpressions.Regex(
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
            System.Text.RegularExpressions.RegexOptions.ExplicitCapture);


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
        public static void ParseFrameHeirarchy(string inputString,
            out int videoId, 
            out int shotId, 
            out int shotStartFrame,
            out int shotEndFrame,
            out int frameNumber, 
            out string extension)
        {
            System.Text.RegularExpressions.Match match = _tokenFormatRegex.Match(inputString);
            if (!match.Success)
            {
                throw new ArgumentException("Unknown interaction token format: " + inputString);
            }

            videoId = int.Parse(match.Groups["videoId"].Value);
            shotId = int.Parse(match.Groups["shotId"].Value);
            shotStartFrame = int.Parse(match.Groups["shotStartFrame"].Value);
            shotEndFrame = int.Parse(match.Groups["shotEndFrame"].Value);
            frameNumber = int.Parse(match.Groups["frameNumber"].Value);
            extension = match.Groups["extension"].Value;

            if (frameNumber < shotStartFrame || frameNumber > shotEndFrame)
            {
                throw new ArgumentException($"Frame number {frameNumber} not in shot range {shotStartFrame}-{shotEndFrame}!");
            }
        }
    }
}
