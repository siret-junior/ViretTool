using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IRankingModule
    {
        ISimilarityModule SimilarityModule { get; }
        IFilteringModule FilteringModule { get; }

        Query CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer IntermediateRanking { get; }
        RankingBuffer OutputRanking { get;  }

        void ComputeRanking(Query query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
