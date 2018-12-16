using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.BusinessLayer.Services
{
    public class DatasetServices
    {
        public DatasetServices(
            IThumbnailService<Thumbnail<byte[]>> thumbnailService,
            IDatasetService datasetService,
            IDescriptorProvider<byte[]> colorSignatureProvider,
            IDescriptorProvider<float[]> semanticVectorProvider)
        {
            ThumbnailService = thumbnailService;
            DatasetService = datasetService;
            ColorSignatureProvider = colorSignatureProvider;
            SemanticVectorProvider = semanticVectorProvider;
        }

        public IThumbnailService<Thumbnail<byte[]>> ThumbnailService { get; }
        public IDatasetService DatasetService { get; }
        public IDescriptorProvider<byte[]> ColorSignatureProvider { get; }
        public IDescriptorProvider<float[]> SemanticVectorProvider { get; }
    }
}
