using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class BiTemporalSimilarityQuery : IEquatable<BiTemporalSimilarityQuery>
    {
        public enum SimilarityModels { Keyword, ColorSketch, FaceSketch, TextSketch, SemanticExample }
        public SimilarityModels PrimarySimilarityModel { get; private set; }

        public FilteredRankingQuery<BiTemporalRankingQuery<KeywordQuery>> KeywordQuery { get; private set; }
        public FilteredRankingQuery<BiTemporalRankingQuery<ColorSketchQuery>> ColorSketchQuery { get; private set; }
        public FilteredRankingQuery<BiTemporalRankingQuery<ColorSketchQuery>> FaceSketchQuery { get; private set; }
        public FilteredRankingQuery<BiTemporalRankingQuery<ColorSketchQuery>> TextSketchQuery { get; private set; }
        public FilteredRankingQuery<BiTemporalRankingQuery<SemanticExampleQuery>> SemanticExampleQuery { get; private set; }
        
        
        public BiTemporalSimilarityQuery(
            FilteredRankingQuery<BiTemporalRankingQuery<KeywordQuery>> keywordQuery, 
            FilteredRankingQuery<BiTemporalRankingQuery<ColorSketchQuery>> colorSketchQuery, 
            FilteredRankingQuery<BiTemporalRankingQuery<ColorSketchQuery>> faceSketchQuery, 
            FilteredRankingQuery<BiTemporalRankingQuery<ColorSketchQuery>> textSketchQuery, 
            FilteredRankingQuery<BiTemporalRankingQuery<SemanticExampleQuery>> semanticExampleQuery, 
            SimilarityModels primarySimilarityModel)
        {
            KeywordQuery = keywordQuery;
            ColorSketchQuery = colorSketchQuery;
            FaceSketchQuery = faceSketchQuery;
            TextSketchQuery = textSketchQuery;
            SemanticExampleQuery = semanticExampleQuery;
            PrimarySimilarityModel = primarySimilarityModel;
        }


        public bool Equals(BiTemporalSimilarityQuery other)
        {
            return KeywordQuery.Equals(other.KeywordQuery) &&
                   ColorSketchQuery.Equals(other.ColorSketchQuery) &&
                   FaceSketchQuery.Equals(other.ColorSketchQuery) &&
                   TextSketchQuery.Equals(other.ColorSketchQuery) &&
                   SemanticExampleQuery.Equals(other.SemanticExampleQuery) &&
                   PrimarySimilarityModel.Equals(other.PrimarySimilarityModel);
        }
    }
}
