using System.Collections.ObjectModel;

namespace ViretTool.DataLayer.DataModel
{
    /// <summary>
    /// Represents a single video of the source video dataset.
    /// </summary>
    public class Video
    {
        public readonly int Id;

        public Dataset ParentDataset { get; internal set; }
        public ReadOnlyCollection<Shot> Shots { get; private set; }
        public ReadOnlyCollection<Frame> Frames { get; private set; }


        public Video(int globalId)
        {
            Id = globalId;
        }


        public override string ToString()
        {
            return "VideoId: " + Id.ToString("00000");
        }


        internal void SetShotMappings(Shot[] shots)
        {
            Shots = new ReadOnlyCollection<Shot>(shots);
            for (int i = 0; i < shots.Length; i++)
            {
                Shot shot = shots[i];
                shot.SetParentVideoMapping(this, i);
            }
        }

        internal void SetFrameMappings(Frame[] frames)
        {
            Frames = new ReadOnlyCollection<Frame>(frames);
            for (int i = 0; i < frames.Length; i++)
            {
                Frame frame = frames[i];
                frame.SetParentVideoMapping(this, i);
            }
        }
    }
}
