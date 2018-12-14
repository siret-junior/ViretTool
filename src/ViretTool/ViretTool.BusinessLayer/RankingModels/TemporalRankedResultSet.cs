using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class TemporalRankedResultSet
    {
        public TemporalQuery TemporalQuery { get; private set; }
        public RankedFrame[][] TemporalResultSets { get; private set; }


        public TemporalRankedResultSet(TemporalQuery temporalQuery, RankedFrame[][] temporalResultSets)
        {
            TemporalQuery = temporalQuery;
            TemporalResultSets = temporalResultSets;
        }
    }
}
