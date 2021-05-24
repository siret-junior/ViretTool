using System.Collections.Generic;

namespace Viret.DataModel
{
    /// <summary>
    /// Represents a single video shot of a video (a sequential set of frames).
    /// </summary>
    public class Shot
    {
        public readonly int Id;
        public readonly Video ParentVideo;
        public readonly int IdInVideo;
        public readonly int StartFrameNumber;
        public readonly int EndFrameNumber;
        public readonly List<Keyframe> Keyframes = new List<Keyframe>();


        public Shot(Video parentVideo, int idInVideo, int startFrameNumber, int endFrameNumber)
        {
            Id = parentVideo.ParentDataset.Shots.Count;
            ParentVideo = parentVideo;
            IdInVideo = idInVideo;
            StartFrameNumber = startFrameNumber;
            EndFrameNumber = endFrameNumber;
            parentVideo.Shots.Add(this);
            parentVideo.ParentDataset.Shots.Add(this);
        }


        public override string ToString()
        {
            return "Shot: " + IdInVideo.ToString("00000")
                + ", Video: " + ParentVideo.Id.ToString("00000");
        }
    }
}
