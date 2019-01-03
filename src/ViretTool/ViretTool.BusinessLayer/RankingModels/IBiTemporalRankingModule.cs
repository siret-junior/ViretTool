using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IBiTemporalRankingModule
    {
        IBiTemporalSimilarityModule SimilarityModule { get; }
        IFilteringModule FormerFilteringModule { get; }
        IFilteringModule LatterFilteringModule { get; }

        BiTemporalQuery CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer IntermediateRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeRanking(BiTemporalQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
