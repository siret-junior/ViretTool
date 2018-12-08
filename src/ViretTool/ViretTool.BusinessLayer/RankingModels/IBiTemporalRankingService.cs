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
    interface IBiTemporalRankingService<TSimpleQuery, TSimpleResult, TTemporalQuery, TTemporalResult>
    {
        IRankingService<TSimpleQuery, TSimpleResult> PrimaryRankingService { get; }
        IRankingService<TSimpleQuery, TSimpleResult> SecondaryRankingService { get; }

        TTemporalQuery CachedQuery { get; }
        TTemporalResult CachedResultSet { get; }

        TTemporalResult ComputeRankedResultSet(TSimpleQuery query);
        TTemporalResult ComputeRankedResultSet(TemporalQuery query);
    }
}
