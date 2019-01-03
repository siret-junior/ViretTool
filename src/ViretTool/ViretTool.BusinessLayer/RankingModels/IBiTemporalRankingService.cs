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
    public interface IBiTemporalRankingService<TQuery, TResult>
    {
        IBiTemporalRankingModule<TQuery> RankingModule { get; }

        TQuery CachedQuery { get; }
        TResult CachedResultSet { get; }
        
        TResult ComputeRankedResultSet(BiTemporalQuery query);
    }
}
