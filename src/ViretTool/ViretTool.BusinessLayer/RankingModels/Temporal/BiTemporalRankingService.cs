using System;
using System.Collections.Generic;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRankingService : IBiTemporalRankingService
    {
        public IDatasetService DatasetService { get; private set; }

        public IBiTemporalRankingModule BiTemporalRankingModule { get; private set; }

        public BiTemporalQuery CachedQuery { get; private set; }
        public BiTemporalRankedResultSet CachedResultSet { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public BiTemporalRankingBuffer OutputRanking { get; private set; }


        public BiTemporalRankingService(
            IDatasetService datasetService, 
            IBiTemporalRankingModule biTemporalRankingModule)
        {
            DatasetService = datasetService;
            BiTemporalRankingModule = biTemporalRankingModule;
        }


        public BiTemporalRankedResultSet ComputeRankedResultSet(BiTemporalQuery query)
        {
            // TODO: restriction to only a half of the dataset -> HasInputChanged (+InputRanking)
            if (!HasQueryChanged(query))
            {
                return CachedResultSet;
            }
            else
            {
                CachedQuery = query;
            }

            InitializeRankingBuffers();

            if (IsQueryEmpty(query))
            {
                return GenerateSequentialRanking();
            }

            try
            {
                // the main computation
                BiTemporalRankingModule.ComputeRanking(query, InputRanking, OutputRanking);
            }
            catch (Exception ex)
            {
                throw;
            }

            // aggregate partial temporal result sets
            List<PairedRankedFrame> formerResultSet = RetrieveRankedResultSet(
                OutputRanking.FormerRankingBuffer.Ranks, OutputRanking.FormerTemporalPairs);
            List<PairedRankedFrame> latterResultSet = RetrieveRankedResultSet(
                OutputRanking.LatterRankingBuffer.Ranks, OutputRanking.LatterTemporalPairs);

            // cache result
            CachedResultSet = new BiTemporalRankedResultSet(query, formerResultSet, latterResultSet);
            return CachedResultSet;
        }


        private BiTemporalRankedResultSet GenerateSequentialRanking()
        {
            // TODO: restriction to only a half of the dataset
            int itemCount = DatasetService.FrameCount;
            List<PairedRankedFrame> rankedFrames = new List<PairedRankedFrame>(itemCount);
            for (int i = 0; i < itemCount; i++)
            {
                rankedFrames.Add(new PairedRankedFrame(i, itemCount - i, -1));
            }

            return new BiTemporalRankedResultSet(null, rankedFrames, rankedFrames);
        }

        private bool HasQueryChanged(BiTemporalQuery query)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery);
        }

        private void InitializeRankingBuffers()
        {
            // create input ranking if neccessary
            if (InputRanking == null 
                || InputRanking.Ranks.Length != DatasetService.FrameCount)
            {
                InputRanking = RankingBuffer.Zeros(
                    "BiTemporalRankingService-InputRanking", DatasetService.FrameCount);
                OutputRanking = BiTemporalRankingBuffer.Zeros(
                    "BiTemporalRankingService-OutputRanking", DatasetService.FrameCount);
            }
        }

        private bool IsQueryEmpty(BiTemporalQuery query)
        {
            return query == null;
        }

        private static List<PairedRankedFrame> RetrieveRankedResultSet(float[] primaryRanks, int[] primaryTemporalPairs)
        {
            // retrieve filtered result set
            List<PairedRankedFrame> resultSet = new List<PairedRankedFrame>(primaryRanks.Length);
            for (int itemId = 0; itemId < primaryRanks.Length; itemId++)
            {
                if (primaryRanks[itemId] != float.MinValue)
                {
                    resultSet.Add(new PairedRankedFrame(itemId, primaryRanks[itemId], primaryTemporalPairs[itemId]));
                }
            }

            // sort descending
            resultSet.Sort((rankedFrame1, rankedFrame2) => rankedFrame2.CompareTo(rankedFrame1));

            return resultSet;
        }
    }
}
