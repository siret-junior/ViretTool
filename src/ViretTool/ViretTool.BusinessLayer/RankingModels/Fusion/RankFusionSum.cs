﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Fusion
{
    public class RankFusionSum : IRankFusion
    {
        public Ranking OutputRanking { get; set; }


        public void ComputeRanking(Ranking[] rankings)
        {
            Ranking resultRanking = Ranking.Zeros(rankings[0].Ranks.Length);

            Parallel.For(0, resultRanking.Ranks.Length, itemId =>
            {
                for (int iRanking = 0; iRanking < rankings.Length; iRanking++)
                {
                    float itemRank = rankings[iRanking].Ranks[itemId];
                    if (itemRank == float.MinValue)
                    {
                        resultRanking.Ranks[itemId] = float.MinValue;
                        return;
                    }
                    resultRanking.Ranks[itemId] += itemRank;
                }
            });

            OutputRanking.Ranks = resultRanking.Ranks;
        }
    }
}