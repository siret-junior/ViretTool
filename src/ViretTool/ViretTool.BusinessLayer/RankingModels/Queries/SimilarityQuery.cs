using System.Collections.Generic;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class SimilarityQuery
    {
        public KeywordQuery KeywordQuery { get; private set; }
        public ColorSketchQuery ColorSketchQuery { get; private set; }
        public SemanticExampleQuery SemanticExampleQuery { get; private set; }
        
        public SimilarityQuery(
            KeywordQuery keywordQuery, 
            ColorSketchQuery colorSketchQuery, 
            SemanticExampleQuery semanticExampleQuery)
        {
            KeywordQuery = keywordQuery;
            ColorSketchQuery = colorSketchQuery;
            SemanticExampleQuery = semanticExampleQuery;
        }


        public override bool Equals(object obj)
        {
            return obj is SimilarityQuery query &&
                   KeywordQuery.Equals(query.KeywordQuery) &&
                   ColorSketchQuery.Equals(query.ColorSketchQuery) &&
                   SemanticExampleQuery.Equals(query.SemanticExampleQuery);
        }

        public override int GetHashCode()
        {
            int hashCode = 2013128003;
            hashCode = hashCode * -1521134295 + KeywordQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + ColorSketchQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + SemanticExampleQuery.GetHashCode();
            return hashCode;
        }
    }
}
