using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalRankingService<TSimpleQuery, TSimpleResult, TTemporalQuery, TTemporalResult>
    {
        IRankingModule PrimaryRankingModule { get; }
        IRankingModule SecondaryRankingModule { get; }

        TTemporalQuery CachedQuery { get; }
        TTemporalResult CachedResultSet { get; }

        TTemporalResult ComputeRankedResultSet(TSimpleQuery query);
        TTemporalResult ComputeRankedResultSet(TemporalQuery query);
    }
}
