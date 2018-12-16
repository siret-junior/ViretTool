using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Fusion
{
    //public class RankFusionWeightedSum : IRankFusion
    //{
    //    public RankingBuffer OutputRanking { get; set; }

    //    public void ComputeRanking(RankingBuffer[] rankings, float[] weights)
    //    {
    //        RankingBuffer resultRanking = RankingBuffer.Zeros(rankings[0].Ranks.Length);

    //        Parallel.For(0, resultRanking.Ranks.Length, itemId =>
    //        {
    //            for (int iRanking = 0; iRanking < rankings.Length; iRanking++)
    //            {
    //                float itemRank = rankings[iRanking].Ranks[itemId];
    //                if (itemRank < 0)
    //                {
    //                    resultRanking.Ranks[itemId] = -1;
    //                    return;
    //                }

    //                if (weights == null)
    //                {
    //                    resultRanking.Ranks[itemId] += itemRank;
    //                }
    //                else
    //                {
    //                    resultRanking.Ranks[itemId] += itemRank * weights[iRanking];
    //                }
    //            }
    //        });

    //        OutputRanking.Ranks = resultRanking.Ranks;
    //        OutputRanking.IsUpdated = true;
    //    }

    //    public void ComputeRanking(RankingBuffer[] rankings)
    //    {
    //        ComputeRanking(rankings, null);
    //    }
    //}
}
