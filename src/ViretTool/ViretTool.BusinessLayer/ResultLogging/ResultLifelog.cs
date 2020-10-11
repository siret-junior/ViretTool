using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class ResultLifelog : ResultBase
    {
        public string Item { get; set; }


        public ResultLifelog(string item, float score, int rank) : base(score, rank)
        {
            Item = item;
        }
    }
}
