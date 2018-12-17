using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
using ViretTool.BusinessLayer.Services;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class BiTemporalRankingService 
        : IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet>
    {
        public BiTemporalRankingService(
            IDatasetService datasetService,
            IRankingModule primaryRankingModule,
            IRankingModule secondaryRankingModule,
            IFilteringModule filteringModule)
        {
            DatasetService = datasetService;
            PrimaryRankingModule = primaryRankingModule;
            SecondaryRankingModule = secondaryRankingModule;
            FilteringModule = filteringModule;
        }

        public IDatasetService DatasetService { get; }

        public IRankingModule PrimaryRankingModule { get; }
        public IRankingModule SecondaryRankingModule { get; }
        public IFilteringModule FilteringModule { get; }

        public TemporalQuery CachedQuery { get; private set; }
        public TemporalRankedResultSet CachedResultSet { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }

        public TemporalRankedResultSet ComputeRankedResultSet(TemporalQuery query)
        {
            if ((query == null && CachedQuery == null) 
                || query.Equals(CachedQuery))
            {
                return CachedResultSet;
            }

            // create input ranking if neccessary
            if (InputRanking == null)
            {
                InputRanking = RankingBuffer.Zeros("InitialRanking", DatasetService.FrameCount);
                OutputRanking = RankingBuffer.Zeros("OutputRanking", DatasetService.FrameCount);
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
