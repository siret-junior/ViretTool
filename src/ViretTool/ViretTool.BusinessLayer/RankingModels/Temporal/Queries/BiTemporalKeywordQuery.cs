using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Queries
{
    public class BiTemporalKeywordQuery
    {
        public KeywordQuery PrimaryQuery { get; private set; }
        public KeywordQuery SecondaryQuery { get; private set; }


        public BiTemporalKeywordQuery(KeywordQuery primaryQuery, KeywordQuery secondaryQuery)
        {
            PrimaryQuery = primaryQuery;
            SecondaryQuery = secondaryQuery;
        }

        public override bool Equals(object obj)
        {
            return obj is BiTemporalKeywordQuery query &&
                   PrimaryQuery.Equals(query.PrimaryQuery) &&
                   SecondaryQuery.Equals(query.SecondaryQuery);
        }

        public override int GetHashCode()
        {
            int hashCode = -1702951762;
            hashCode = hashCode * -1521134295 + PrimaryQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + SecondaryQuery.GetHashCode();
            return hashCode;
        }
    }
}
