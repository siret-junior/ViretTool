using System.Collections.Generic;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class TemporalQuery
    {
        public Query[] TemporalQueries { get; private set; }
        

        public TemporalQuery(Query[] temporalQueries)
        {
            TemporalQueries = temporalQueries;
        }
        

        public override bool Equals(object obj)
        {
            return obj is TemporalQuery query &&
                   EqualityComparer<Query[]>.Default.Equals(TemporalQueries, query.TemporalQueries);
        }

        public override int GetHashCode()
        {
            return 802700933 + EqualityComparer<Query[]>.Default.GetHashCode(TemporalQueries);
        }
    }
}
