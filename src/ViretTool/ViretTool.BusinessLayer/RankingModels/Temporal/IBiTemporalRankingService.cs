using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalRankingService
    {
        IBiTemporalRankingModule BiTemporalRankingModule { get; }

        BiTemporalQuery CachedQuery { get; }
        BiTemporalRankedResultSet CachedResultSet { get; }

        RankingBuffer InputRanking { get; }
        BiTemporalRankingBuffer OutputRanking { get; }
        
        BiTemporalRankedResultSet ComputeRankedResultSet(BiTemporalQuery query);
    }
}
