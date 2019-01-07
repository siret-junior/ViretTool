using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering
{
    public class RankFilteringModule : IRankFilteringModule
    {
        private const int SAMPLE_SIZE = 1000;
        private const int RANDOM_SEED = 42;
        private Random _random = new Random(RANDOM_SEED);
        private int[] _sampleIndexes;
        private double[] _sampleValues;
        List<float> notFilteredRanks;

        public ThresholdFilteringQuery CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }


        public RankFilteringModule()
        {
        }


        public void ComputeFiltering(ThresholdFilteringQuery query,
            RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;
            InitializeIntermediateBuffers();

            if (!HasQueryOrInputChanged(query, inputRanking))
            {
                // nothing changed, OutputRanking contains cached data from previous computation
                OutputRanking.IsUpdated = false;
                return;
            }
            else
            {
                CachedQuery = query;
                OutputRanking.IsUpdated = true;
            }

            if (IsQueryEmpty(query))
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
                return;
            }

            switch (query.FilterState)
            {
                case ThresholdFilteringQuery.State.IncludeAboveThreshold:
                    IncludeAbove((float)query.Threshold, inputRanking, outputRanking);
                    return;
                case ThresholdFilteringQuery.State.ExcludeAboveThreshold:
                    //ExcludeAbove((float)query.Threshold, inputRanking, outputRanking);
                    //return;
                case ThresholdFilteringQuery.State.Off:
                default:
                    throw new NotImplementedException(
                        $"Filter state {Enum.GetName(typeof(FilterState), query.FilterState)} not expected.");
            }
        }

        private void InitializeIntermediateBuffers()
        {
            if (_sampleIndexes == null)
            {
                _sampleIndexes = new int[SAMPLE_SIZE];
                _sampleValues = new double[SAMPLE_SIZE];

                // initialize sampling indexes
                
                for (int i = 0; i < SAMPLE_SIZE; i++)
                {
                    _sampleIndexes[i] = _random.Next(InputRanking.Ranks.Length);
                }
            }
        }

        private bool HasQueryOrInputChanged(ThresholdFilteringQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(ThresholdFilteringQuery query)
        {
            return query == null
                || (query.FilterState == ThresholdFilteringQuery.State.Off);
        }

        private void IncludeAbove(float percentageOfDatabase,
            RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            double threshold = EstimateRankValueThreshold(inputRanking, percentageOfDatabase);

            // set mask using the threshold
            Parallel.For(0, InputRanking.Ranks.Length, i =>
            {
                if (InputRanking.Ranks[i] >= threshold)
                {
                    OutputRanking.Ranks[i] = InputRanking.Ranks[i];
                }
                else
                {
                    OutputRanking.Ranks[i] = float.MinValue;
                }
            });
        }

        private void ExcludeAbove(float threshold, RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            throw new NotImplementedException();
        }

        private double EstimateRankValueThreshold(RankingBuffer inputRanking, float percentageOfDatabase)
        {
            // TODO: parallel for optimization for small loop bodies using range partitioner

            // estimate a rank value threshold for a given percentageOfDatabase
            //Parallel.For(0, SAMPLE_SIZE, i =>
            //{
            //    _sampleValues[i] = InputRanking.Ranks[_sampleIndexes[i]];

            //    // TODO: are we sampling in the entire dataset or just in the active subset (first half of dataset)
            //    if (_sampleValues[i] == float.MinValue)
            //    {
            //        throw new NotImplementedException("RankedDatasetFilter sampling of a previously filtered frame.");
            //    }
            //});

            _random = new Random(RANDOM_SEED);

            // prepare list for not filtered ranks
            if (notFilteredRanks == null || notFilteredRanks.Capacity < InputRanking.Ranks.Length)
            {
                notFilteredRanks = new List<float>(InputRanking.Ranks.Length);
            }
            else
            {
                notFilteredRanks.Clear();
            }

            // extract not filtered ranks
            for (int i = 0; i < InputRanking.Ranks.Length; i++)
            {
                float rank = InputRanking.Ranks[i];
                if (rank != float.MinValue)
                {
                    notFilteredRanks.Add(rank);
                }
            }

            float nonFilteredPercentage = ((float)notFilteredRanks.Count()) / InputRanking.Ranks.Length;
            if (nonFilteredPercentage > percentageOfDatabase)
            {
                // too much results, sample
                for (int i = 0; i < SAMPLE_SIZE; i++)
                {
                    _sampleValues[i] = notFilteredRanks[_random.Next(SAMPLE_SIZE)];
                }
                float percentageInSubset = percentageOfDatabase / nonFilteredPercentage;
                Array.Sort(_sampleValues, (a, b) => b.CompareTo(a));
                double threshold = _sampleValues[(int)((SAMPLE_SIZE - 1) * percentageInSubset)];
                return threshold;
            }
            else
            {
                // too few results, return all (set threshold to the smallest value)
                float minValue = float.MaxValue;
                for (int i = 0; i < SAMPLE_SIZE; i++)
                {
                    if (notFilteredRanks[i] != float.MinValue && notFilteredRanks[i] < minValue)
                    {
                        minValue = notFilteredRanks[i];
                    }
                }
                return minValue;
            }
        }
    }
}
