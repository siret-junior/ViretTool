namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IBiTemporalSimilarityModel<TQuery, TSimilarityModel>
    {
        TSimilarityModel SimilarityModel { get; }

        TQuery CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer IntermediateRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeRanking(TQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
