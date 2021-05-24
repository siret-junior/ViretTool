namespace Viret.DataModel
{
    /// <summary>
    /// A representative frame selected from a shot in a video.
    /// </summary>
    public class Keyframe
    {
        public readonly int Id;

        // Parent mappings
        public readonly Video ParentVideo;
        public readonly int IdInVideo;
        public readonly int FrameInVideo;
        public readonly Shot ParentShot;
        public readonly int IdInShot;


        public Keyframe(Video parentVideo, int idInVideo, int frameInVideo, Shot parentShot, int idInShot)
        {
            Id = parentVideo.ParentDataset.Keyframes.Count;
            ParentVideo = parentVideo;
            IdInVideo = idInVideo;
            ParentShot = parentShot;
            IdInShot = idInShot;
            FrameInVideo = frameInVideo;
            ParentVideo.Keyframes.Add(this);
            ParentShot.Keyframes.Add(this);
            ParentVideo.ParentDataset.Keyframes.Add(this);
        }


        public override string ToString()
        {
            return "Keyframe: " + Id.ToString("00000000")
                + ", Frame: " + FrameInVideo.ToString("00000")
                + ", Video: " + ParentVideo.Id.ToString("00000")
                + ", Shot: " + ParentShot.Id.ToString("00000");
        }
    }
}
