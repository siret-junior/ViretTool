using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class ResultBase
    {
        public float Score { get; set; }
        public int Rank { get; set; }


        public ResultBase(float score, int rank)
        {
            Score = score;
            Rank = rank;
        }
    }
}
