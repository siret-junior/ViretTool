using System;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Queries
{
    public class BiTemporalSimilarityQuery : IEquatable<BiTemporalSimilarityQuery>
    {
        public BiTemporalModelQuery<KeywordQuery> KeywordQuery { get; private set; }
        public BiTemporalModelQuery<ColorSketchQuery> ColorSketchQuery { get; private set; }
        public BiTemporalModelQuery<ColorSketchQuery> FaceSketchQuery { get; private set; }
        public BiTemporalModelQuery<ColorSketchQuery> TextSketchQuery { get; private set; }
        public BiTemporalModelQuery<SemanticExampleQuery> SemanticExampleQuery { get; private set; }
        
        
        public BiTemporalSimilarityQuery(
            BiTemporalModelQuery<KeywordQuery> keywordQuery, 
            BiTemporalModelQuery<ColorSketchQuery> colorSketchQuery, 
            BiTemporalModelQuery<ColorSketchQuery> faceSketchQuery, 
            BiTemporalModelQuery<ColorSketchQuery> textSketchQuery, 
            BiTemporalModelQuery<SemanticExampleQuery> semanticExampleQuery)
        {
            KeywordQuery = keywordQuery;
            ColorSketchQuery = colorSketchQuery;
            FaceSketchQuery = faceSketchQuery;
            TextSketchQuery = textSketchQuery;
            SemanticExampleQuery = semanticExampleQuery;
        }


        public bool Equals(BiTemporalSimilarityQuery other)
        {
            return KeywordQuery.Equals(other.KeywordQuery) &&
                   ColorSketchQuery.Equals(other.ColorSketchQuery) &&
                   FaceSketchQuery.Equals(other.ColorSketchQuery) &&
                   TextSketchQuery.Equals(other.ColorSketchQuery) &&
                   SemanticExampleQuery.Equals(other.SemanticExampleQuery);
        }
    }
}
