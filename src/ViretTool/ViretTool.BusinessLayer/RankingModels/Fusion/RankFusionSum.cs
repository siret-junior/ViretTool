using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Fusion
{
    public class RankFusionSum : IRankFusion
    {
        public RankingBuffer[] InputRankings { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }


        public void ComputeRanking(RankingBuffer[] inputRankings, RankingBuffer outputRanking)
        {  
            Parallel.For(0, outputRanking.Ranks.Length, itemId =>
            {
                outputRanking.Ranks[itemId] = 0;
                for (int iRanking = 0; iRanking < inputRankings.Length; iRanking++)
                {
                    float itemRank = inputRankings[iRanking].Ranks[itemId];
                    if (itemRank == float.MinValue)
                    {
                        outputRanking.Ranks[itemId] = float.MinValue;
                        return;
                    }
                    outputRanking.Ranks[itemId] += itemRank;
                }
            });

            OutputRanking.Ranks = outputRanking.Ranks;
        }
    }
}
