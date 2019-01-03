using System.Collections.Generic;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRankedResultSet
    {
        public TemporalQuery TemporalQuery { get; private set; }

        public List<PairedRankedFrame> PrimaryTemporalResultSet { get; private set; }
        public List<PairedRankedFrame> SecondaryTemporalResultSet { get; private set; }
        

        public BiTemporalRankedResultSet(
            TemporalQuery temporalQuery, 
            List<PairedRankedFrame> primaryTemporalResultSet, 
            List<PairedRankedFrame> secondaryTemporalResultSet)
        {
            TemporalQuery = temporalQuery;

            PrimaryTemporalResultSet = primaryTemporalResultSet;
            SecondaryTemporalResultSet = secondaryTemporalResultSet;
        }
    }
}
