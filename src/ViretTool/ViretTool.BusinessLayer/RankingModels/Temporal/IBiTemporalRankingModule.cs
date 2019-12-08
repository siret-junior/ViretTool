using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Fusion;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalRankingModule
    {
        IBiTemporalSimilarityModule BiTemporalSimilarityModule { get; }
        IFusionModule FormerFusionModule { get; }
        IFusionModule LatterFusionModule { get; }
        IFilteringModule FormerFilteringModule { get; }
        IFilteringModule LatterFilteringModule { get; }

        BiTemporalQuery CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        BiTemporalRankingBuffer KeywordIntermediateRanking { get; }
        BiTemporalRankingBuffer ColorSketchIntermediateRanking { get; }
        BiTemporalRankingBuffer FaceSketchIntermediateRanking { get; }
        BiTemporalRankingBuffer TextSketchIntermediateRanking { get; }
        BiTemporalRankingBuffer SemanticExampleIntermediateRanking { get; }
        BiTemporalRankingBuffer IntermediateFusionRanking { get; }
        BiTemporalRankingBuffer OutputRanking { get; }

        void ComputeRanking(BiTemporalQuery query, 
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking);
    }
}
