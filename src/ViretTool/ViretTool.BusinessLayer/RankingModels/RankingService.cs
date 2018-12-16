using System;
using System.Collections.Generic;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels
{
    internal class RankingService : IRankingService<Query, RankedResultSet>
    {
        public int FrameCount { get; private set; }

        public IRankingModule RankingModule { get; internal set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }


        public RankingService(int frameCount, IRankingModule rankingModule)
        {
            FrameCount = frameCount;
            RankingModule = rankingModule;

            InputRanking = RankingBuffer.Zeros("InitialRanking", frameCount);
            OutputRanking = RankingBuffer.Zeros("FinalRanking", frameCount);
        }


        public RankedResultSet ComputeRankedResultSet(Query query)
        {
            // TODO: see BiTemporalRankingService

            RankingModule.ComputeRanking(query, InputRanking, OutputRanking);
            float[] ranks = RankingModule.OutputRanking.Ranks;

            // retrieve filtered result
            List<RankedFrame> rankedFrames = GetUnfilteredFrames(ranks);

            // sort ranks descending
            rankedFrames.Sort((rankedFrame1, rankedFrame2) => rankedFrame2.Rank.CompareTo(rankedFrame1.Rank));
            
            return new RankedResultSet(query, rankedFrames);
        }

        private static List<RankedFrame> GetUnfilteredFrames(float[] ranks)
        {
            List<RankedFrame> accumulator = new List<RankedFrame>(ranks.Length);
            for (int itemId = 0; itemId < ranks.Length; itemId++)
            {
                if (ranks[itemId] != float.MinValue)
                {
                    accumulator.Add(new RankedFrame(itemId, ranks[itemId]));
                }
            }

            return accumulator;
        }
    }
}
