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

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRankingServiceExternal 
        : IBiTemporalRankingServiceExternal<Query, RankedResultSet, TemporalQuery, BiTemporalRankedResultSet>
    {
        public IDatasetService DatasetService { get; }

        public IRankingModule PrimaryRankingModule { get; }
        public IRankingModule SecondaryRankingModule { get; }
        public IFilteringModule FilteringModule { get; }

        public TemporalQuery CachedQuery { get; private set; }
        public BiTemporalRankedResultSet CachedResultSet { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }


        public BiTemporalRankingServiceExternal(
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
        

        public BiTemporalRankedResultSet ComputeRankedResultSet(TemporalQuery query)
        {
            if ((query == null && CachedQuery == null) 
                || query.Equals(CachedQuery))
            {
                return CachedResultSet;
            }

            // create ranking buffers if neccessary
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
                List<RankedFrame> primaryRankedFrames = new List<RankedFrame>(ranks.Length);
                for (int itemId = 0; itemId < ranks.Length; itemId++)
                {
                    if (ranks[itemId] != float.MinValue)
                    {
                        primaryRankedFrames.Add(new RankedFrame(itemId, ranks[itemId]));
                    }
                }

                // sort descending
                primaryRankedFrames.Sort(
                    (rankedFrame1, rankedFrame2) => rankedFrame2.Rank.CompareTo(rankedFrame1.Rank));

                // TODO:
                return new BiTemporalRankedResultSet(query,
                    primaryRankedFrames, primaryRankedFrames, null, null);
            }
            else
            {
                // default sequential ranking (in reverse order)
                int itemCount = PrimaryRankingModule.InputRanking.Ranks.Length;
                List<RankedFrame> primaryRankedFrames = new List<RankedFrame>(itemCount);
                for (int i = 0; i < itemCount; i++)
                {
                    primaryRankedFrames.Add(new RankedFrame(i, itemCount - i));
                }

                // TODO:
                return new BiTemporalRankedResultSet(query,
                    primaryRankedFrames, primaryRankedFrames, null, null);
            }
        }

        public BiTemporalRankedResultSet ComputeRankedResultSet(Query query)
        {
            return ComputeRankedResultSet(new TemporalQuery(new Query[2] { query, null }));
        }
    }
}
