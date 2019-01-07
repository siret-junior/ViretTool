using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Fusion
{
    public class BiTemporalRankFusionFilters : IBiTemporalRankFusionFilters
    {
        public IDatasetServicesManager DatasetServicesManager { get; }

        public RankingBuffer PrimaryInputRanking { get; private set; }
        public RankingBuffer SecondaryInputRanking { get; private set; }
        public BiTemporalRankingBuffer OutputRanking { get; private set; }

        private readonly int _temporalContextLength;

        public BiTemporalRankFusionFilters(
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
                // skip already filtered frames
                if (primaryInputRanking.Ranks[frameId] == float.MinValue)
                {
                    singleOutputRanking.Ranks[frameId] = float.MinValue;
                    singleOutputPairs[frameId] = -1;
                    return;
                }
                
                // include frame only if the temporal range also includes it
                bool isIncludedInSecondary = false;
                int maxPairRankId = -1;
                for (int iContext = 1; iContext <= _temporalContextLength; iContext++)
                {
                    int iPairFrame = frameId + iContext;
                    if (iPairFrame > lastFrameIdsInVideoForFrameId[frameId]) break;

                    float pairRank = secondaryInputRanking.Ranks[iPairFrame];
                    if (pairRank != float.MinValue)
                    {
                        isIncludedInSecondary = true;
                        maxPairRankId = iPairFrame;
                        break;
                    }
                }

                // compute result
                if (isIncludedInSecondary)
                {
                    singleOutputRanking.Ranks[frameId] = 0;
                    singleOutputPairs[frameId] = maxPairRankId;
                }
                else
                {
                    singleOutputRanking.Ranks[frameId] = float.MinValue;
                    singleOutputPairs[frameId] = -1;
                }
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
                // skip already filtered frames
                if (primaryInputRanking.Ranks[frameId] == float.MinValue)
                {
                    singleOutputRanking.Ranks[frameId] = float.MinValue;
                    singleOutputPairs[frameId] = -1;
                    return;
                }

                // include frame only if the temporal range also includes it
                bool isIncludedInSecondary = false;
                int maxPairRankId = -1;
                for (int iContext = 1; iContext <= _temporalContextLength; iContext++)
                {
                    int iPairFrame = frameId - iContext;
                    if (iPairFrame < firstFrameIdsInVideoForFrameId[frameId]) break;

                    float pairRank = secondaryInputRanking.Ranks[iPairFrame];
                    if (pairRank != float.MinValue)
                    {
                        isIncludedInSecondary = true;
                        maxPairRankId = iPairFrame;
                        break;
                    }
                }
                
                // compute result
                if (isIncludedInSecondary)
                {
                    singleOutputRanking.Ranks[frameId] = 0;
                    singleOutputPairs[frameId] = maxPairRankId;
                }
                else
                {
                    singleOutputRanking.Ranks[frameId] = float.MinValue;
                    singleOutputPairs[frameId] = -1;
                }
            });
            singleOutputRanking.IsUpdated = true;
        }
    }
}
