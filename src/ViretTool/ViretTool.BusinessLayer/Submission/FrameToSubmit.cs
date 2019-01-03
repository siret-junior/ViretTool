namespace ViretTool.BusinessLayer.Submission
{
    public class FrameToSubmit
    {
        public FrameToSubmit(int videoId, int frameId, int shotId)
        {
            VideoId = videoId;
            FrameId = frameId;
            ShotId = shotId;
        }

        public int VideoId { get; }
        public int FrameId { get; }
        public int ShotId { get; }
    }
}