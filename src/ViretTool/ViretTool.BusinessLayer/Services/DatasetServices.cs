using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.BusinessLayer.Services
{
    /// <summary>
    /// Class for storing services bound to a specific dataset
    /// </summary>
    public class DatasetServices
    {
        // All services that need disposing or closing functionality have to be passed here
        public DatasetServices(
            IThumbnailService<Thumbnail<byte[]>> thumbnailService,
            IDatasetService datasetService,
            IDescriptorProvider<byte[]> colorSignatureProvider,
            IDescriptorProvider<float[]> semanticVectorProvider,
            IBiTemporalRankingService rankingService,
            IDatasetParameters datasetParameters,
            IDescriptorProvider<LifelogFrameMetadata> lifelogDescriptorProvider,
            IInitialDisplayProvider initialDisplayProvider)
        {
            ThumbnailService = thumbnailService;
            DatasetService = datasetService;
            ColorSignatureProvider = colorSignatureProvider;
            SemanticVectorProvider = semanticVectorProvider;
            RankingService = rankingService;
            DatasetParameters = datasetParameters;
            LifelogDescriptorProvider = lifelogDescriptorProvider;
            InitialDisplayProvider = initialDisplayProvider;
        }

        public IThumbnailService<Thumbnail<byte[]>> ThumbnailService { get; }
        public IDatasetService DatasetService { get; }
        public IDescriptorProvider<byte[]> ColorSignatureProvider { get; }
        public IDescriptorProvider<float[]> SemanticVectorProvider { get; }
        public IBiTemporalRankingService RankingService { get; }
        public IDatasetParameters DatasetParameters { get; }
        public IDescriptorProvider<LifelogFrameMetadata> LifelogDescriptorProvider { get; }
        public IInitialDisplayProvider InitialDisplayProvider { get; }
    }
}
