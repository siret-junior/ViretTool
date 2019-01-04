using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class FilteredRankingQuery<TQuery> 
        : IEquatable<FilteredRankingQuery<TQuery>> 
        where TQuery : IQuery
    {
        enum State { Include, Exclude, Off }

        TQuery RankingQuery { get; }
        float IncludeTopKPercentage { get; }


        public FilteredRankingQuery(TQuery rankingQuery, float allowTopKPercent)
        {
            RankingQuery = rankingQuery;
            IncludeTopKPercentage = allowTopKPercent;
        }


        public bool Equals(FilteredRankingQuery<TQuery> other)
        {
            return RankingQuery.Equals(other.RankingQuery) &&
                IncludeTopKPercentage.Equals(other.IncludeTopKPercentage);
        }

        public override bool Equals(object obj)
        {
            return obj is FilteredRankingQuery<TQuery> query &&
                   Equals(query);
        }
    }
}
