
namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    /// <summary>
    /// Extends ThumbnailReader by preloading all thumbnail images encoded in JPEG format
    /// and storing them into memory (an array) for fast access.
    /// Used primarily on machines that have relatively slow random access to external storage (i.e. hard disk drives).
    /// </summary>
    public class ThumbnailReaderPreloaded : ThumbnailReader
    {
        private readonly ThumbnailDataJpeg[] _thumbnailsPreloaded;

        public ThumbnailReaderPreloaded(string filePath) : base(filePath)
        {
            // preload thumbnails
            _thumbnailsPreloaded = new ThumbnailDataJpeg[ThumbnailCount];
            for (int globalId = 0; globalId < ThumbnailCount; globalId++)
            {
                _thumbnailsPreloaded[globalId] = base.ReadVideoThumbnail(globalId);
            }
        }

        public override ThumbnailDataJpeg ReadVideoThumbnail(int globalId)
        {
            return _thumbnailsPreloaded[globalId];
        }

        public override ThumbnailDataJpeg[] ReadVideoThumbnails(int videoId)
        {
            int globalIdStart = VideoOffsets[videoId];
            int videoLength = VideoFrameCounts[videoId];
            int globalIdEnd = globalIdStart + videoLength;
            ThumbnailDataJpeg[] thumbnails = new ThumbnailDataJpeg[videoLength];

            for (int globalId = globalIdStart; globalId < globalIdEnd; globalId++)
            {
                thumbnails[globalId - globalIdStart] = _thumbnailsPreloaded[globalId];
            }

            return thumbnails;
        }
    }
}
