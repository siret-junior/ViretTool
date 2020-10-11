using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class ResultLogLifelog : ResultLogBase
    {
        public ResultLifelog[] Results { get; set; }

        public ResultLogLifelog(BiTemporalQuery query, ResultLifelog[] results) : base(query)
        {
            Results = results;
        }
    }
}
