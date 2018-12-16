using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IFilteringModule
    {
        FilteringQuery CachedQuery { get; }
        RankingBuffer InputRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeRanking(FilteringQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
