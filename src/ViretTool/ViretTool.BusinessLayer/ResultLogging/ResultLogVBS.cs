﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.Submission;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class ResultLogVBS : ResultLogBase
    {
        [JsonProperty(Order = 10)]
        public ResultVBS[] Results { get; set; }


        public ResultLogVBS(BiTemporalQuery query, ResultVBS[] results) : base(query)
        {
            Results = results;
        }

        
    }
}
