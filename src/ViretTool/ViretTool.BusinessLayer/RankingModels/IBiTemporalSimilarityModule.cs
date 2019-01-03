using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IBiTemporalSimilarityModule
    {
        IFilteredRankingModule<FilteredRankingQuery<KeywordQuery>, IBiTemporalSimilarityModel<IKeywordModel>> FilteredKeywordModel { get; }
        IKeywordModel<KeywordQuery> KeywordModel { get; }
        IColorSketchModel<ColorSketchQuery> ColorSketchModel { get; }
        ISemanticExampleModel<SemanticExampleQuery> SemanticExampleModel { get; }
        
        BiTemporalSimilarityQuery CachedQuery { get; }

        void ComputeRanking(BiTemporalSimilarityQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
