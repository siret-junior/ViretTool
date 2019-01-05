using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Fusion
{
    public interface IFusionModule
    {
        IRankFilteringModule KeywordRankFilteringModule { get; }
        IRankFilteringModule ColorSketchRankFilteringModule { get; }
        IRankFilteringModule FaceSketchRankFilteringModule { get; }
        IRankFilteringModule TextSketchRankFilteringModule { get; }
        IRankFilteringModule SemanticExampleRankFilteringModule { get; }

        FusionQuery CachedQuery { get; }
        
        RankingBuffer KeywordRanking { get; }
        RankingBuffer ColorSketchRanking { get; }
        RankingBuffer FaceSketchRanking { get; }
        RankingBuffer TextSketchRanking { get; }
        RankingBuffer SemanticExampleRanking { get; }
        
        RankingBuffer KeywordIntermediateRanking { get; }
        RankingBuffer ColorSketchIntermediateRanking { get; }
        RankingBuffer FaceSketchIntermediateRanking { get; }
        RankingBuffer TextSketchIntermediateRanking { get; }
        RankingBuffer SemanticExampleIntermediateRanking { get; }

        RankingBuffer OutputRanking { get; }


        void ComputeRanking(FusionQuery query,
            RankingBuffer keywordRanking,
            RankingBuffer colorSketchRanking,
            RankingBuffer faceSketchRanking,
            RankingBuffer textSketchRanking,
            RankingBuffer semanticExampleRanking,
            RankingBuffer outputRanking,
            // TODO: remove and create ITemporalFusionModule
            int[] temporalPairs);
    }
}
