using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class TemporalRankedResultSet
    {
        public BiTemporalQuery TemporalQuery { get; private set; }
        public RankedFrame[][] TemporalResultSets { get; private set; }


        public TemporalRankedResultSet(BiTemporalQuery temporalQuery, RankedFrame[][] temporalResultSets)
        {
            TemporalQuery = temporalQuery;
            TemporalResultSets = temporalResultSets;
        }
    }
}
