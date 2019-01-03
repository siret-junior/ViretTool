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
        public RankingBuffer PrimaryRankingBuffer { get; }
        public RankingBuffer SecondaryRankingBuffer { get; }

        public int[] PrimaryPairs { get; }
        public int[] SecondaryPairs { get; }
        
        public bool IsUpdated => PrimaryRankingBuffer.IsUpdated || SecondaryRankingBuffer.IsUpdated;
        

        public BiTemporalRankingBuffer(string name,
            RankingBuffer primaryRankingBuffer, RankingBuffer secondaryRankingBuffer)
        {
            Name = name;
            PrimaryRankingBuffer = primaryRankingBuffer;
            SecondaryRankingBuffer = secondaryRankingBuffer;

            PrimaryPairs = new int[primaryRankingBuffer.Ranks.Length];
            SecondaryPairs = new int[primaryRankingBuffer.Ranks.Length];
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
                RankingBuffer.Value(name + "-primary", itemCount, value),
                RankingBuffer.Value(name + "-secondary", itemCount, value));
        }
    }
}
