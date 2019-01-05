using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity
{
    public interface IBiTemporalSimilarityModel<TQuery, TSimilarityModel, TRankFusion>
        where TQuery : IQuery
        where TSimilarityModel : ISimilarityModel<TQuery>
        where TRankFusion : IBiTemporalRankFusion
    {
        TSimilarityModel FormerSimilarityModel { get; }
        TSimilarityModel LatterSimilarityModel { get; }
        IBiTemporalRankFusion BiTemporalRankFusion { get; }

        BiTemporalModelQuery<TQuery> CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer FormerIntermediateRanking { get; }
        RankingBuffer LatterIntermediateRanking { get; }
        BiTemporalRankingBuffer OutputRanking { get; }

        void ComputeRanking(BiTemporalModelQuery<TQuery> query,
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking);
    }
}
