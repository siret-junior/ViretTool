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
            InputRankings = inputRankings;
            OutputRanking = outputRanking;
            MaxNormalizeRankings(inputRankings);

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
                    else
                    {
                        outputRanking.Ranks[itemId] += itemRank;
                    }
                }
            });
            OutputRanking.IsUpdated = true;
        }


        private static void MaxNormalizeRankings(RankingBuffer[] rankings)
        {
            for (int i = 0; i < rankings.Length; i++)
            {
                MaxNormalizeRanking(rankings[i]);
            }
        }

        private static void MaxNormalizeRanking(RankingBuffer ranking)
        {
            double maxRank;
            double minRank;

            FindMinimumAndMaximum(ranking, out maxRank, out minRank);

            // prepare offset and normalizer
            double offset = -minRank;
            double normalizer = (maxRank != minRank)
                ? 1.0 / Math.Abs(maxRank - minRank)
                : 0;

            // normalize to range [0..1]
            Parallel.For(0, ranking.Ranks.Length, index =>
            {
                if (ranking.Ranks[index] != float.MinValue)
                {
                    ranking.Ranks[index] += (float)offset;
                    ranking.Ranks[index] *= (float)normalizer;
                }
            });
        }

        private static void FindMinimumAndMaximum(RankingBuffer ranking, out double maximum, out double minimum)
        {
            maximum = float.MinValue;
            minimum = float.MaxValue;

            for (int i = 0; i < ranking.Ranks.Length; i++)
            {
                double rank = ranking.Ranks[i];
                maximum = (rank > maximum) ? rank : maximum;
                minimum = (rank < minimum && rank != float.MinValue) ? rank : minimum;
            }
        }
    }
}
