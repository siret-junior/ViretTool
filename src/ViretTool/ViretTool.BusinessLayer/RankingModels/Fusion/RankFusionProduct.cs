using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Fusion
{
    public class RankFusionProduct : IRankFusion
    {
        public Ranking ComputeRanking(Ranking[] rankings)
        {
            Ranking resultRanking = Ranking.Ones(rankings[0].Ranks.Length);

            Parallel.For(0, resultRanking.Ranks.Length, itemId =>
            {
                for (int iRanking = 0; iRanking < rankings.Length; iRanking++)
                {
                    float itemRank = rankings[iRanking].Ranks[itemId];
                    if (itemRank == float.MinValue)
                    {
                        resultRanking.Ranks[itemId] = -1;
                        return;
                    }
                    resultRanking.Ranks[itemId] *= itemRank;
                }
            });

            return resultRanking;
        }
    }
}
