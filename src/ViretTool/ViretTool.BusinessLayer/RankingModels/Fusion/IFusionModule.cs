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
        
        // TODO: add intermediate rankings if neccessary
        RankingBuffer KeywordRanking { get; }
        RankingBuffer ColorSketchRanking { get; }
        RankingBuffer FaceSketchRanking { get; }
        RankingBuffer TextSketchRanking { get; }
        RankingBuffer SemanticExampleRanking { get; }

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
