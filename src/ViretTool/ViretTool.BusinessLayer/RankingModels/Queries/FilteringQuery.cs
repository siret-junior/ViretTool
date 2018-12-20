namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class FilteringQuery
    {
        public ThresholdFilteringQuery ColorSaturationQuery { get; private set; }
        public ThresholdFilteringQuery PercentOfBlackQuery { get; private set; }

        public ThresholdFilteringQuery ColorSketchFilteringQuery { get; private set; }
        public ThresholdFilteringQuery KeywordFilteringQuery { get; private set; }
        public ThresholdFilteringQuery SemanticExampleFilteringQuery { get; private set; }

        public CountFilteringQuery CountFilteringQuery { get; private set; }


        public FilteringQuery(
            ThresholdFilteringQuery colorSaturationQuery, 
            ThresholdFilteringQuery percentOfBlackQuery, 
            ThresholdFilteringQuery colorSketchFilteringQuery, 
            ThresholdFilteringQuery keywordFilteringQuery, 
            ThresholdFilteringQuery semanticExampleFilteringQuery)
        {
            ColorSaturationQuery = colorSaturationQuery;
            PercentOfBlackQuery = percentOfBlackQuery;
            ColorSketchFilteringQuery = colorSketchFilteringQuery;
            KeywordFilteringQuery = keywordFilteringQuery;
            SemanticExampleFilteringQuery = semanticExampleFilteringQuery;
        }

        public override bool Equals(object obj)
        {
            return obj is FilteringQuery query &&
                   ColorSaturationQuery.Equals(query.ColorSaturationQuery) &&
                   PercentOfBlackQuery.Equals(query.PercentOfBlackQuery) &&
                   ColorSketchFilteringQuery.Equals(query.ColorSketchFilteringQuery) &&
                   KeywordFilteringQuery.Equals(query.KeywordFilteringQuery) &&
                   SemanticExampleFilteringQuery.Equals(query.SemanticExampleFilteringQuery);
        }

        public override int GetHashCode()
        {
            int hashCode = -1378753317;
            hashCode = hashCode * -1521134295 + ColorSaturationQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + PercentOfBlackQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + ColorSketchFilteringQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + KeywordFilteringQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + SemanticExampleFilteringQuery.GetHashCode();
            return hashCode;
        }
    }
}
