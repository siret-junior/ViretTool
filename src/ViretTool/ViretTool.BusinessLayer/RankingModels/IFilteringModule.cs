using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IFilteringModule
    {
        FilteringQuery CachedQuery { get; }
        Ranking InputRanking { get; }
        Ranking OutputRanking { get; }

        void ComputeRanking(FilteringQuery query);
    }
}
