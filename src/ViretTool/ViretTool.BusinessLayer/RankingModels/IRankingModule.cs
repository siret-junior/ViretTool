using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IRankingModule
    {
        ISimilarityModule SimilarityModule { get; }
        IFilteringModule FilteringModule { get; }

        Query CachedQuery { get; }
        Ranking InputRanking { get; }
        Ranking OutputRanking { get; }

        void ComputeRanking(Query query);
    }
}
