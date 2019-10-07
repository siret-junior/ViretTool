namespace ViretTool.DataLayer.DataModel
{
    /// <summary>
    /// A representative frame selected from a shot in a video.
    /// </summary>
    public class Frame
    {
        /// <summary>
        /// The global identifier of the representative frame in a given dataset.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Number of the frame in the source video.
        /// </summary>
        public int FrameNumber { get; internal set; }


        // Parent mappings
        public Video ParentVideo { get; private set; }
        public int IdInVideo { get; private set; }

        public Shot ParentShot { get; private set; }
        public int IdInShot { get; private set; }

        //public Group ParentGroup { get; private set; }
        public int IdInGroup { get; private set; }

        
        public Frame(int globalId, int frameNumber = -1)
        {
            Id = globalId;
            FrameNumber = frameNumber;
        }


        public override string ToString()
        {
            return "FrameId: " + Id.ToString()
                + ", Video: " + ParentVideo.Id.ToString("00000")
                + ", Shot: " + ParentShot.Id.ToString("00000");
                //+ ", Group: " + ParentGroup.Id.ToString("00000");
        }


        internal void SetParentVideoMapping(Video parentVideo, int idInVideo)
        {
            ParentVideo = parentVideo;
            IdInVideo = idInVideo;
        }

        internal void SetParentShotMapping(Shot parentShot, int idInShot)
        {
            ParentShot = parentShot;
            IdInShot = idInShot;
        }

        //internal void SetParentGroupMapping(Group parentGroup, int idInGroup)
        //{
        //    ParentGroup = parentGroup;
        //    IdInGroup = idInGroup;
        //}


        internal void WithFrameNumber(int frameNumber)
        {
            FrameNumber = frameNumber;
        }

    }
}
