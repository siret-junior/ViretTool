using System;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Queries
{
    
    public class BiTemporalModelQuery<TQuery> 
        : IQuery, IEquatable<BiTemporalModelQuery<TQuery>>
        where TQuery : IQuery
    {
        public TQuery FormerQuery { get; }
        public TQuery LatterQuery { get; }
        

        public BiTemporalModelQuery(TQuery formerQuery, TQuery latterQuery)
        {
            FormerQuery = formerQuery;
            LatterQuery = latterQuery;
        }


        public bool Equals(BiTemporalModelQuery<TQuery> other)
        {
            return FormerQuery.Equals(other.FormerQuery) &&
                LatterQuery.Equals(other.LatterQuery);
        }

        public override bool Equals(object obj)
        {
            return obj is BiTemporalModelQuery<TQuery> query &&
                   Equals(query);
        }
    }
}
