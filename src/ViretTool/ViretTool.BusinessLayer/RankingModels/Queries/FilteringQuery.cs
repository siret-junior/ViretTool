using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class FilteringQuery : IFilteringQuery, IEquatable<FilteringQuery>
    {
        public ThresholdFilteringQuery ColorSaturationQuery { get; private set; }
        public ThresholdFilteringQuery PercentOfBlackQuery { get; private set; }
        
        public CountFilteringQuery CountFilteringQuery { get; private set; }


        public FilteringQuery(
            ThresholdFilteringQuery colorSaturationQuery, 
            ThresholdFilteringQuery percentOfBlackQuery, 
            CountFilteringQuery countFilteringQuery)
        {
            ColorSaturationQuery = colorSaturationQuery;
            PercentOfBlackQuery = percentOfBlackQuery;
            CountFilteringQuery = countFilteringQuery;
        }


        public bool Equals(FilteringQuery other)
        {
            return ColorSaturationQuery.Equals(other.ColorSaturationQuery) &&
                PercentOfBlackQuery.Equals(other.PercentOfBlackQuery) &&
                CountFilteringQuery.Equals(other.CountFilteringQuery);
        }
    }
}
