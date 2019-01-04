using System.Collections.Generic;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRankedResultSet
    {
        public BiTemporalQuery TemporalQuery { get; private set; }

        public List<PairedRankedFrame> FormerTemporalResultSet { get; private set; }
        public List<PairedRankedFrame> LatterTemporalResultSet { get; private set; }

        public BiTemporalRankedResultSet(
            BiTemporalQuery temporalQuery, 
            List<PairedRankedFrame> formerTemporalResultSet,
            List<PairedRankedFrame> latterTemporalResultSet)
        {
            TemporalQuery = temporalQuery;
            FormerTemporalResultSet = formerTemporalResultSet;
            LatterTemporalResultSet = latterTemporalResultSet;
        }
    }
}
