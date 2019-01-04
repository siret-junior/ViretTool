using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class FusionQuery : IFusionQuery, IEquatable<FusionQuery>
    {
        public enum SimilarityModels { Keyword, ColorSketch, FaceSketch, TextSketch, SemanticExample }

        public SimilarityModels SortingSimilarityModel { get; }

        public ThresholdFilteringQuery KeywordFilteringQuery { get; }
        public ThresholdFilteringQuery ColorSketchFilteringQuery { get; }
        public ThresholdFilteringQuery FaceSketchFilteringQuery { get; }
        public ThresholdFilteringQuery TextSketchFilteringQuery { get; }
        public ThresholdFilteringQuery SemanticExampleFilteringQuery { get; }


        public FusionQuery(
            SimilarityModels sortingSimilarityModel, 
            ThresholdFilteringQuery keywordFilteringQuery, 
            ThresholdFilteringQuery colorSketchFilteringQuery, 
            ThresholdFilteringQuery faceSketchFilteringQuery, 
            ThresholdFilteringQuery textSketchFilteringQuery, 
            ThresholdFilteringQuery semanticExampleFilteringQuery)
        {
            SortingSimilarityModel = sortingSimilarityModel;
            KeywordFilteringQuery = keywordFilteringQuery;
            ColorSketchFilteringQuery = colorSketchFilteringQuery;
            FaceSketchFilteringQuery = faceSketchFilteringQuery;
            TextSketchFilteringQuery = textSketchFilteringQuery;
            SemanticExampleFilteringQuery = semanticExampleFilteringQuery;
        }

        public bool Equals(FusionQuery other)
        {
            return SortingSimilarityModel.Equals(other.SortingSimilarityModel)
                && KeywordFilteringQuery.Equals(other.KeywordFilteringQuery)
                && ColorSketchFilteringQuery.Equals(other.ColorSketchFilteringQuery)
                && FaceSketchFilteringQuery.Equals(other.FaceSketchFilteringQuery)
                && TextSketchFilteringQuery.Equals(other.TextSketchFilteringQuery)
                && SemanticExampleFilteringQuery.Equals(other.SemanticExampleFilteringQuery);
        }
    }
}
