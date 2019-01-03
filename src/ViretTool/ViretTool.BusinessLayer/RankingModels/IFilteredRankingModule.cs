namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IFilteredRankingModule<TQuery, TRankingModule>
    {
        TRankingModule RankingModule { get; }

        TQuery CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer IntermediateRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeRanking(TQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
