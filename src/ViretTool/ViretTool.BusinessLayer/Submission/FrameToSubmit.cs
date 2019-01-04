namespace ViretTool.BusinessLayer.Submission
{
    public class FrameToSubmit
    {
        public FrameToSubmit(int videoId, int frameNumber)
        {
            VideoId = videoId;
            FrameNumber = frameNumber;
        }

        public int VideoId { get; }
        public int FrameNumber { get; }
    }
}
