using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public enum PrimaryQuery { Former, Latter }

    public class BiTemporalRankingQuery<TQuery> 
        : IRankingQuery, IEquatable<BiTemporalRankingQuery<TQuery>>
        where TQuery : IRankingQuery
    {
        TQuery FormerQuery { get; }
        TQuery LatterQuery { get; }
        PrimaryQuery PrimaryQuery { get; }


        public BiTemporalRankingQuery(TQuery formerQuery, TQuery latterQuery, PrimaryQuery primaryQuery)
        {
            FormerQuery = formerQuery;
            LatterQuery = latterQuery;
            PrimaryQuery = primaryQuery;
        }


        public bool Equals(BiTemporalRankingQuery<TQuery> other)
        {
            return FormerQuery.Equals(other.FormerQuery) &&
                LatterQuery.Equals(other.LatterQuery) &&
                PrimaryQuery.Equals(other.PrimaryQuery);
        }

        public override bool Equals(object obj)
        {
            return obj is BiTemporalRankingQuery<TQuery> query &&
                   Equals(query);
        }
    }
}
