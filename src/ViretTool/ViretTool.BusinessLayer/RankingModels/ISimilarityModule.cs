//using ViretTool.BusinessLayer.RankingModels.Queries;
//using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

//namespace ViretTool.BusinessLayer.RankingModels
//{
//    public interface ISimilarityModule
//    {
//        IKeywordModel<KeywordQuery> KeywordModel { get; }
//        IColorSketchModel<ColorSketchQuery> ColorSketchModel { get; }
//        ISemanticExampleModel<SemanticExampleQuery> SemanticExampleModel { get; }
//        IRankFusion RankFusion { get; }

//        SimilarityQuery CachedQuery { get; }
//        RankingBuffer InputRanking { get; }
//        RankingBuffer KeywordIntermediateRanking { get; }
//        RankingBuffer ColorIntermediateRanking { get; }
//        RankingBuffer SemanticExampleIntermediateRanking { get; }
//        RankingBuffer OutputRanking { get; }

//        void ComputeRanking(SimilarityQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking);
//    }
//}
