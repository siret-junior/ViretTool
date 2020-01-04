using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class Result
    {
        public int VideoId { get; set; }
        public int Frame { get; set; }
        public float Score { get; set; }
        public int Rank { get; set; }


        public Result(int videoId, int frameNumber, float score, int rank)
        {
            VideoId = videoId;
            Frame = frameNumber;
            Score = score;
            Rank = rank;
        }
    }
}
