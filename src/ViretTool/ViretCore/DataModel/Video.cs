
using System.Collections.Generic;

namespace Viret.DataModel
{
    /// <summary>
    /// Represents a single video of the source video dataset.
    /// </summary>
    public class Video
    {
        public readonly int Id;
        public readonly Dataset ParentDataset;
        public readonly List<Shot> Shots = new List<Shot>();
        public readonly List<Keyframe> Keyframes = new List<Keyframe>();


        public Video(Dataset parentDataset, int globalId)
        {
            Id = globalId;
            ParentDataset = parentDataset;
            parentDataset.Videos.Add(this);
        }


        public override string ToString()
        {
            return "Video: " + Id.ToString("00000");
        }
    }
}
