using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalSimilarityModule
    {
        IBiTemporalSimilarityModel<KeywordQuery, IKeywordModel> KeywordModel { get; }
        IBiTemporalSimilarityModel<ColorSketchQuery, IColorSketchModel> ColorSketchModel { get; }
        IBiTemporalSimilarityModel<ColorSketchQuery, IFaceSketchModel> FaceSketchModel { get; }
        IBiTemporalSimilarityModel<ColorSketchQuery, ITextSketchModel> TextSketchModel { get; }
        IBiTemporalSimilarityModel<SemanticExampleQuery, ISemanticExampleModel> SemanticExampleModel { get; }
        
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
