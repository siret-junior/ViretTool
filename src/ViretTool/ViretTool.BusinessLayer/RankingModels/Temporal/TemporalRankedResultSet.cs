using ViretTool.BusinessLayer.RankingModels.Queries;

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
