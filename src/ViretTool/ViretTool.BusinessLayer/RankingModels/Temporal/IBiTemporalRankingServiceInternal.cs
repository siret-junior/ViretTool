using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalRankingServiceInternal<TSimpleQuery, TSimpleResult, TTemporalQuery, TTemporalResult>
    {
        IBiTemporalRankingModule BiTemporalRankingModule { get; }

        TTemporalQuery CachedQuery { get; }
        TTemporalResult CachedResultSet { get; }

        TTemporalResult ComputeRankedResultSet(TSimpleQuery query);
        TTemporalResult ComputeRankedResultSet(TTemporalQuery query);
    }
}
