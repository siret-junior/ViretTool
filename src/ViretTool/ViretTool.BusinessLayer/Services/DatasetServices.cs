using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.BusinessLayer.Services
{
    public class DatasetServices
    {
        public DatasetServices(IThumbnailService<Thumbnail<byte[]>> thumbnailService, IDatasetService datasetService)
        {
            ThumbnailService = thumbnailService;
            DatasetService = datasetService;
        }

        public IThumbnailService<Thumbnail<byte[]>> ThumbnailService { get; }
        public IDatasetService DatasetService { get; }
    }
}
