using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.BusinessLayer.Services
{
    public class DatasetServices
    {
        public DatasetServices(IThumbnailService<Thumbnail<byte[]>> thumbnailService)
        {
            ThumbnailService = thumbnailService;
        }

        public IThumbnailService<Thumbnail<byte[]>> ThumbnailService { get; }
    }
}
