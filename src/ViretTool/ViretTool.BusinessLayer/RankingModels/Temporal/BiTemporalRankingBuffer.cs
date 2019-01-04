using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRankingBuffer
    {
        public string Name { get; }
        public RankingBuffer FormerRankingBuffer { get; }
        public RankingBuffer LatterRankingBuffer { get; }

        public int[] FormerTemporalPairs { get; }
        public int[] LatterTemporalPairs { get; }
        
        public bool IsUpdated => FormerRankingBuffer.IsUpdated || LatterRankingBuffer.IsUpdated;
        

        public BiTemporalRankingBuffer(string name,
            RankingBuffer formerRankingBuffer, RankingBuffer latterRankingBuffer)
        {
            Name = name;
            FormerRankingBuffer = formerRankingBuffer;
            LatterRankingBuffer = latterRankingBuffer;

            FormerTemporalPairs = new int[formerRankingBuffer.Ranks.Length];
            LatterTemporalPairs = new int[formerRankingBuffer.Ranks.Length];
        }


        public static BiTemporalRankingBuffer Zeros(string name, int itemCount)
        {
            return Value(name, itemCount, 0);
        }

        public static BiTemporalRankingBuffer Ones(string name, int itemCount)
        {
            return Value(name, itemCount, 1);
        }

        public static BiTemporalRankingBuffer Value(string name, int itemCount, float value)
        {
            return new BiTemporalRankingBuffer(name,
                RankingBuffer.Value(name + "-Former", itemCount, value),
                RankingBuffer.Value(name + "-Latter", itemCount, value));
        }
    }
}
