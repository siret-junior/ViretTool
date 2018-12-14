using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IRankingModule
    {
        ISimilarityModule SimilarityModule { get; }
        IFilteringModule FilteringModule { get; }

        Query CachedQuery { get; }
        Ranking InputRanking { get; set; }
        Ranking OutputRanking { get; set; }

        void ComputeRanking(Query query);
    }
}
