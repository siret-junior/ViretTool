//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.BusinessLayer.Datasets;
//using ViretTool.BusinessLayer.RankingModels.Queries;

//namespace ViretTool.BusinessLayer.RankingModels.Temporal
//{
//    class BiTemporalRankingServiceInternal
//        : IBiTemporalRankingServiceInternal<Query, RankedResultSet, TemporalQuery, BiTemporalRankedResultSet>
//    {
//        public IDatasetService DatasetService { get; }

//        public IBiTemporalRankingModule BiTemporalRankingModule { get; }
        
//        public TemporalQuery CachedQuery { get; private set; }
//        public BiTemporalRankedResultSet CachedResultSet { get; private set; }

//        public RankingBuffer InputRanking { get; private set; }
//        public BiTemporalRankingBuffer OutputRanking { get; private set; }


//        public BiTemporalRankingServiceInternal(
//            IDatasetService datasetService,
//            IBiTemporalRankingModule biTemporalRankingModule)
//        {
//            DatasetService = datasetService;
//            BiTemporalRankingModule = biTemporalRankingModule;
//        }


//        public BiTemporalRankedResultSet ComputeRankedResultSet(TemporalQuery query)
//        {
//            if ((query == null && CachedQuery == null)
//                || query.Equals(CachedQuery))
//            {
//                return CachedResultSet;
//            }

//            // create ranking buffers if neccessary
//            if (InputRanking == null)
//            {
//                InputRanking = RankingBuffer.Zeros("InitialRanking", DatasetService.FrameCount);
//                OutputRanking = BiTemporalRankingBuffer.Zeros("OutputRanking", DatasetService.FrameCount);
//            }

//            if (query != null)
//            {
//                // compute ranking
//                BiTemporalRankingModule.ComputeRanking(query, InputRanking, OutputRanking);
                

//                // retrieve filtered result
//                List<PairedRankedFrame> primaryResultAccumulator = RetrieveFilteredResult(
//                    OutputRanking.FormerRankingBuffer.Ranks, OutputRanking.FormerTemporalPairs);

//                List<PairedRankedFrame> secondaryResultAccumulator = RetrieveFilteredResult(
//                    OutputRanking.LatterRankingBuffer.Ranks, OutputRanking.LatterTemporalPairs);
                

//                return new BiTemporalRankedResultSet(query,
//                    primaryResultAccumulator, secondaryResultAccumulator);
//            }
//            else
//            {
//                // default sequential ranking
//                int itemCount = InputRanking.Ranks.Length;
//                List<PairedRankedFrame> primaryResultAccumulator = new List<PairedRankedFrame>(itemCount);
//                for (int i = 0; i < itemCount; i++)
//                {
//                    primaryResultAccumulator.Add(new PairedRankedFrame(i, itemCount - i, -1));
//                }

//                return new BiTemporalRankedResultSet(query, primaryResultAccumulator, null);
//            }
//        }

//        private List<PairedRankedFrame> RetrieveFilteredResult(float[] ranks, int[] primaryPairs)
//        {
//            List<PairedRankedFrame> resultAccumulator = new List<PairedRankedFrame>(ranks.Length);

//            for (int itemId = 0; itemId < ranks.Length; itemId++)
//            {
//                if (ranks[itemId] != float.MinValue)
//                {
//                    resultAccumulator.Add(
//                        new PairedRankedFrame(itemId, ranks[itemId], primaryPairs[itemId]));
//                }
//            }

//            // sort descending
//            resultAccumulator.Sort(
//                (rankedFrame1, rankedFrame2) => rankedFrame2.Rank.CompareTo(rankedFrame1.Rank));

//            return resultAccumulator;
//        }

//        public BiTemporalRankedResultSet ComputeRankedResultSet(Query query)
//        {
//            return ComputeRankedResultSet(new TemporalQuery(new Query[2] { query, null }));
//        }
//    }
//}
