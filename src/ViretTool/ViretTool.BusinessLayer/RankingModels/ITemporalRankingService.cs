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
    interface ITemporalRankingService<TResult>
    {
        IRankingService<TResult> PrimaryRankingService { get; }
        IRankingService<TResult> SecondaryRankingService { get; }

        TemporalQuery CachedQuery { get; }
        TResult CachedFilteredRanking { get; }

        TResult ComputeRankedResultSet(Query query);
        TResult ComputeRankedResultSet(TemporalQuery query);
    }
}
