using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Queries
{
    public class BiTemporalColorSketchQuery
    {
        public ColorSketchQuery PrimaryQuery { get; private set; }
        public ColorSketchQuery SecondaryQuery { get; private set; }


        public BiTemporalColorSketchQuery(ColorSketchQuery primaryQuery, ColorSketchQuery secondaryQuery)
        {
            PrimaryQuery = primaryQuery;
            SecondaryQuery = secondaryQuery;
        }

        public override bool Equals(object obj)
        {
            return obj is BiTemporalColorSketchQuery query &&
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
