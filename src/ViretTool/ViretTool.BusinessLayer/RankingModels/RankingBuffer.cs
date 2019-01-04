using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class RankingBuffer
    {
        public string Name { get; }
        public float[] Ranks { get; set; }
        public bool IsUpdated { get; set; }


        public RankingBuffer(string name, float[] ranks, bool isUpdated = true)
        {
            Name = name;
            Ranks = ranks;
            IsUpdated = isUpdated;
        }

        //public RankingBuffer(RankingBuffer ranking, bool isUpdated = true)
        //{
        //    Ranks = new float[ranking.Ranks.Length];
        //    for (int i = 0; i < Ranks.Length; i++)
        //    {
        //        Ranks[i] = ranking.Ranks[i];
        //    }
        //    IsUpdated = isUpdated;
        //}


        public static RankingBuffer Zeros(string name, int itemCount)
        {
            return Value(name, itemCount, 0);
        }

        public static RankingBuffer Ones(string name, int itemCount)
        {
            return Value(name, itemCount, 1);
        }

        public static RankingBuffer Value(string name, int itemCount, float value)
        {
            float[] ranks = new float[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                ranks[i] = value;
            }

            return new RankingBuffer(name, ranks);
        }
    }
}
