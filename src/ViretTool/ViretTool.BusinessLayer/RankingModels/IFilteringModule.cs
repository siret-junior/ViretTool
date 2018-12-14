using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IFilteringModule
    {
        FilteringQuery CachedQuery { get; }
        Ranking InputRanking { get; set; }
        Ranking OutputRanking { get; set; }

        void ComputeRanking(FilteringQuery query);
    }
}
