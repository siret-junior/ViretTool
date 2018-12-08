using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class BiTemporalRankingService 
        : IBiTemporalRankingService<Query, RankedFrame[], TemporalQuery, TemporalRankedFrame[]>
    {
        public IRankingService<Query, RankedFrame[]> PrimaryRankingService { get; private set; }
        public IRankingService<Query, RankedFrame[]> SecondaryRankingService { get; private set; }

        public TemporalQuery CachedQuery { get; private set; }
        public TemporalRankedFrame[] CachedResultSet { get; private set; }
        
        public IFilteringModule FilteringModule { get; private set; }
        

        public TemporalRankedFrame[] ComputeRankedResultSet(TemporalQuery query)
        {
            if (query.Equals(CachedQuery) /* TODO initial ranking has not changed */)
            {
                return CachedResultSet;
            }
            else
            {
                // compute partial rankings



                // aggregate temporal rankings


                throw new NotImplementedException();
            }
        }

        public TemporalRankedFrame[] ComputeRankedResultSet(Query query)
        {
            throw new NotImplementedException();
        }
    }
}
