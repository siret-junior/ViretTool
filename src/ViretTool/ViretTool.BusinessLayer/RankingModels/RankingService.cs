using System;
using System.Collections.Generic;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    internal class RankingService : IRankingService<Query, RankedFrame[]>
    {
        public IRankingModule RankingModule { get; internal set; }

        public RankedFrame[] ComputeRankedResultSet(Query query)
        {
            RankingModule.ComputeRanking(query);
            float[] ranks = RankingModule.OutputRanking.Ranks;

            // retrieve filtered result
            List<RankedFrame> accumulator = new List<RankedFrame>(ranks.Length);
            for (int itemId = 0; itemId < ranks.Length; itemId++)
            {
                if (ranks[itemId] != float.MinValue)
                {
                    accumulator.Add(new RankedFrame(itemId, ranks[itemId]));
                }
            }
            RankedFrame[] resultSet = accumulator.ToArray();

            // sort descending
            Array.Sort(resultSet, (rankedFrame1, rankedFrame2) => rankedFrame2.Rank.CompareTo(rankedFrame1.Rank));
            return resultSet;
        }
    }
}
