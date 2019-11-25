namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    public class ThumbnailDataJpeg
    {
        public int VideoId { get; private set; }
        public int FrameNumber { get; private set; }
        public byte[] JpegData { get; private set; }


        public ThumbnailDataJpeg(int videoId, int frameNumber, byte[] jpegData)
        {
            VideoId = videoId;
            FrameNumber = frameNumber;
            JpegData = jpegData;
        }
    }
}
