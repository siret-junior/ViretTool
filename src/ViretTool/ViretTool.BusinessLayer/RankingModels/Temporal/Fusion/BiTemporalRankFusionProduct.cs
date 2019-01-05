using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Fusion
{
    public class BiTemporalRankFusionProduct : IBiTemporalRankFusionProduct
    {
        public IDatasetServicesManager DatasetServicesManager { get; }

        public RankingBuffer PrimaryInputRanking { get; private set; }
        public RankingBuffer SecondaryInputRanking { get; private set; }
        public BiTemporalRankingBuffer OutputRanking { get; private set; }

        private readonly int _temporalContextLength;

        public BiTemporalRankFusionProduct(
            IDatasetServicesManager datasetServicesManager,
            int temporalContextLength = 5)
        {
            DatasetServicesManager = datasetServicesManager;
            _temporalContextLength = temporalContextLength;
        }


        public void ComputeRanking(
            RankingBuffer primaryInputRanking, RankingBuffer secondaryInputRanking,
            BiTemporalRankingBuffer outputRanking)
        {
            PrimaryInputRanking = primaryInputRanking;
            SecondaryInputRanking = secondaryInputRanking;
            OutputRanking = outputRanking;

            // TODO: translate negative ranks
            // removed: translation is performed in the model
            //TranslateRanks();

            ComputeSingleRankingForward(PrimaryInputRanking, SecondaryInputRanking,
                OutputRanking.FormerRankingBuffer, OutputRanking.FormerTemporalPairs);

            ComputeSingleRankingBackward(SecondaryInputRanking, PrimaryInputRanking,
                OutputRanking.LatterRankingBuffer, OutputRanking.LatterTemporalPairs);
        }


        private void ComputeSingleRankingForward(
            RankingBuffer primaryInputRanking, RankingBuffer secondaryInputRanking,
            RankingBuffer singleOutputRanking, int[] singleOutputPairs)
        {
            int[] lastFrameIdsInVideoForFrameId =
                DatasetServicesManager.CurrentDataset.DatasetService.GetLastFrameIdsInVideoForFrameId();

            Parallel.For(0, primaryInputRanking.Ranks.Length, frameId =>
            {
                // find the highest secondary rank in the temporal context
                float maxPairRank = float.MinValue;
                int maxPairRankId = -1;
                for (int iPairFrame = frameId + 1; iPairFrame <= lastFrameIdsInVideoForFrameId[frameId]; iPairFrame++)
                {
                    float pairRank = secondaryInputRanking.Ranks[iPairFrame];
                    if (pairRank > maxPairRank)
                    {
                        maxPairRank = pairRank;
                        maxPairRankId = iPairFrame;
                    }
                }
                // ranks without any pair rank will not be improved, nor degraded
                if (maxPairRankId == -1)
                {
                    maxPairRank = 0;
                }

                // compute result
                singleOutputRanking.Ranks[frameId] = primaryInputRanking.Ranks[frameId] * maxPairRank;
                singleOutputPairs[frameId] = maxPairRankId;
            });
            singleOutputRanking.IsUpdated = true;
        }

        private void ComputeSingleRankingBackward(
            RankingBuffer primaryInputRanking, RankingBuffer secondaryInputRanking,
            RankingBuffer singleOutputRanking, int[] singleOutputPairs)
        {
            int[] firstFrameIdsInVideoForFrameId =
                DatasetServicesManager.CurrentDataset.DatasetService.GetFirstFrameIdsInVideoForFrameId();

            Parallel.For(0, primaryInputRanking.Ranks.Length, frameId =>
            {
                // find the highest secondary rank in the temporal context
                float maxPairRank = float.MinValue;
                int maxPairRankId = -1;
                for (int iPairFrame = frameId - 1; iPairFrame >= firstFrameIdsInVideoForFrameId[frameId]; iPairFrame--)
                {
                    float pairRank = secondaryInputRanking.Ranks[iPairFrame];
                    if (pairRank > maxPairRank)
                    {
                        maxPairRank = pairRank;
                        maxPairRankId = iPairFrame;
                    }
                }
                // ranks without any pair rank will not be improved, nor degraded
                if (maxPairRankId == -1)
                {
                    maxPairRank = 0;
                }

                // compute result
                singleOutputRanking.Ranks[frameId] = primaryInputRanking.Ranks[frameId] * maxPairRank;
                singleOutputPairs[frameId] = maxPairRankId;
            });
            singleOutputRanking.IsUpdated = true;
        }
    }
}
