using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface ISimilarityModule
    {
        IKeywordModel<KeywordQuery> KeywordModel { get; }
        IColorSketchModel<ColorSketchQuery> ColorSketchModel { get; }
        ISemanticExampleModel<SemanticExampleQuery> SemanticExampleModel { get; }
        IRankFusion RankFusion { get; }

        BiTemporalSimilarityQuery CachedQuery { get; }

        void ComputeRanking(BiTemporalSimilarityQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
