﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class BiTemporalRankingService 
        : IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet>
    {
        public Dataset Dataset { get; private set; }

        public IRankingModule PrimaryRankingModule { get; set; }
        public IRankingModule SecondaryRankingModule { get; set; }
        public IFilteringModule FilteringModule { get; set; }

        public TemporalQuery CachedQuery { get; private set; }
        public TemporalRankedResultSet CachedResultSet { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }


        public BiTemporalRankingService(Dataset dataset)
        {
            InputRanking = RankingBuffer.Zeros("InitialRanking", dataset.Frames.Count);
        }


        public TemporalRankedResultSet ComputeRankedResultSet(TemporalQuery query)
        {
            if ((query == null && CachedQuery == null) 
                || query.Equals(CachedQuery))
            {
                return CachedResultSet;
            }

            if (query != null)
            {
                // compute partial rankings
                PrimaryRankingModule.ComputeRanking(query.TemporalQueries[0], InputRanking, OutputRanking);
                // TODO:
                //SecondaryRankingModule.ComputeRanking(query.TemporalQueries[1]);

                float[] ranks = OutputRanking.Ranks;

                // aggregate temporal rankings
                // TODO: secondary temporal query


                // retrieve filtered result
                List<RankedFrame> accumulator = new List<RankedFrame>(ranks.Length);
                for (int itemId = 0; itemId < ranks.Length; itemId++)
                {
                    if (ranks[itemId] != float.MinValue)
                    {
                        accumulator.Add(new RankedFrame(itemId, ranks[itemId]));
                    }
                }
                RankedFrame[] primaryRankedFrames = accumulator.ToArray();

                // sort descending
                Array.Sort(primaryRankedFrames, (rankedFrame1, rankedFrame2) => rankedFrame2.Rank.CompareTo(rankedFrame1.Rank));

                return new TemporalRankedResultSet(query,
                    new RankedFrame[][] { primaryRankedFrames, primaryRankedFrames });
            }
            else
            {
                int itemCount = PrimaryRankingModule.InputRanking.Ranks.Length;
                RankedFrame[] rankedFrames = new RankedFrame[itemCount];
                for (int i = 0; i < itemCount; i++)
                {
                    rankedFrames[i] = new RankedFrame(i, itemCount - i);
                }

                return new TemporalRankedResultSet(query,
                    new RankedFrame[][] { rankedFrames, rankedFrames });
            }
        }

        public TemporalRankedResultSet ComputeRankedResultSet(Query query)
        {
            return ComputeRankedResultSet(new TemporalQuery(new Query[2] { query, null }));
        }
    }
}
