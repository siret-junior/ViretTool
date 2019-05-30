using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public interface ILifelogFilter
    {
        RankingBuffer InputRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeFiltering(LifelogFilteringQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
