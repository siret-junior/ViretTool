using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class ResultVBS : ResultBase
    {
        public int VideoId { get; set; }
        public int Frame { get; set; }


        public ResultVBS(int videoId, int frameNumber, float score, int rank) : base(score, rank)
        {
            VideoId = videoId;
            Frame = frameNumber;
        }
    }
}
