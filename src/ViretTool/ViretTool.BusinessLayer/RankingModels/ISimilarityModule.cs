using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface ISimilarityModule
    {
        IKeywordModel<KeywordQuery> KeywordModel { get; set; }
        IColorSketchModel<ColorSketchQuery> ColorSketchModel { get; set; }
        ISemanticExampleModel<SemanticExampleQuery> SemanticExampleModel { get; set; }
        IRankFusion RankFusion { get; }

        SimilarityQuery CachedQuery { get; }
        Ranking InputRanking { get; set; }
        Ranking OutputRanking { get; set; }

        void ComputeRanking(SimilarityQuery query);
    }
}
