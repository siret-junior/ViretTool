namespace ViretTool.DataLayer.DataModel.Attributes
{
    public class VideoAttributes
    {
        public readonly Video ParentVideo;

        public string FileName { get; private set; }


        public VideoAttributes(Video parentVideo)
        {
            ParentVideo = parentVideo;
        }


        public override string ToString()
        {
            return FileName;
        }


        internal VideoAttributes WithFileName(string fileName)
        {
            FileName = fileName;
            return this;
        }

        // TODO: add attributes
    }
}
