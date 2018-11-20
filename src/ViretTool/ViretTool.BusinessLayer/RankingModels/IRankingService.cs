using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IRankingService<TResult>
    {
        IRankingModule RankingModule { get; }

        TResult ComputeRanking(Query query);
    }
}
