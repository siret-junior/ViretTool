using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Queries;
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
            IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet> rankingService)
        {
            ThumbnailService = thumbnailService;
            DatasetService = datasetService;
            ColorSignatureProvider = colorSignatureProvider;
            SemanticVectorProvider = semanticVectorProvider;
            RankingService = rankingService;
        }

        public IThumbnailService<Thumbnail<byte[]>> ThumbnailService { get; }
        public IDatasetService DatasetService { get; }
        public IDescriptorProvider<byte[]> ColorSignatureProvider { get; }
        public IDescriptorProvider<float[]> SemanticVectorProvider { get; }
        public IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet> RankingService { get; }
    }
}
