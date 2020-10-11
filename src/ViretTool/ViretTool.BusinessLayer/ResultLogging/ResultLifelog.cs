using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class ResultLifelog : ResultBase
    {
        public string Video { get; set; }


        public ResultLifelog(string video, float score, int rank) : base(score, rank)
        {
            Video = video;
        }
    }
}
