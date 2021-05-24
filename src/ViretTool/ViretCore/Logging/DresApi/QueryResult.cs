using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Logging.DresApi
{
    public class QueryResult
    {
        public string Item;
        public int Frame;
        public double Score;
        public int Rank;

        public QueryResult(string item, int frame, double score, int rank)
        {
            Item = item;
            Frame = frame;
            Score = score;
            Rank = rank;
        }
    }
}
