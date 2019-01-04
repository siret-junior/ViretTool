using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering
{
    public interface IRankFilteringModule
    {
        ThresholdFilteringQuery CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer OutputRanking { get;  }

        void ComputeFiltering(ThresholdFilteringQuery query,
            RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
