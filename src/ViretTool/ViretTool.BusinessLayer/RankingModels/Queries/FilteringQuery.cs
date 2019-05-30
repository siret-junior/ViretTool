using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class FilteringQuery : IFilteringQuery, IEquatable<FilteringQuery>
    {
        public FilteringQuery(
            ThresholdFilteringQuery colorSaturationQuery,
            ThresholdFilteringQuery percentOfBlackQuery,
            CountFilteringQuery countFilteringQuery,
            LifelogFilteringQuery lifelogFilteringQuery)
        {
            ColorSaturationQuery = colorSaturationQuery;
            PercentOfBlackQuery = percentOfBlackQuery;
            CountFilteringQuery = countFilteringQuery;
            LifelogFilteringQuery = lifelogFilteringQuery;
        }

        public ThresholdFilteringQuery ColorSaturationQuery { get; }

        public CountFilteringQuery CountFilteringQuery { get; }
        public LifelogFilteringQuery LifelogFilteringQuery { get; }
        public ThresholdFilteringQuery PercentOfBlackQuery { get; }


        public bool Equals(FilteringQuery other)
        {
            return ColorSaturationQuery.Equals(other.ColorSaturationQuery) &&
                   PercentOfBlackQuery.Equals(other.PercentOfBlackQuery) &&
                   CountFilteringQuery.Equals(other.CountFilteringQuery) &&
                   LifelogFilteringQuery.Equals(other.LifelogFilteringQuery);
        }
    }
}
