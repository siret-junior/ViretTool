using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity;
using ViretTool.BusinessLayer.RankingModels.Temporal.Fusion;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalSimilarityModule
    {
        IBiTemporalSimilarityModel
            <KeywordQuery, IKeywordModel, IBiTemporalRankFusionProduct> 
            KeywordModel { get; }
        IBiTemporalSimilarityModel
            <ColorSketchQuery, IColorSketchModel, IBiTemporalRankFusionSum> 
            ColorSketchModel { get; }
        IBiTemporalSimilarityModel
            <ColorSketchQuery, IFaceSketchModel, IBiTemporalRankFusionFilters> 
            FaceSketchModel { get; }
        IBiTemporalSimilarityModel
            <ColorSketchQuery, ITextSketchModel, IBiTemporalRankFusionFilters> 
            TextSketchModel { get; }
        IBiTemporalSimilarityModel
            <SemanticExampleQuery, ISemanticExampleModel, IBiTemporalRankFusionSum> 
            SemanticExampleModel { get; }
        
        BiTemporalSimilarityQuery CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        BiTemporalRankingBuffer KeywordOutputRanking { get; }
        BiTemporalRankingBuffer ColorSketchOutputRanking { get; }
        BiTemporalRankingBuffer FaceSketchOutputRanking { get; }
        BiTemporalRankingBuffer TextSketchOutputRanking { get; }
        BiTemporalRankingBuffer SemanticExampleOutputRanking { get; }


        void ComputeRanking(
            BiTemporalSimilarityQuery query,
            RankingBuffer inputRanking,
            BiTemporalRankingBuffer keywordOutputRanking,
            BiTemporalRankingBuffer colorSketchOutputRanking,
            BiTemporalRankingBuffer faceSketchOutputRanking,
            BiTemporalRankingBuffer textSketchOutputRanking,
            BiTemporalRankingBuffer semanticExampleOutputRanking);
    }
}
