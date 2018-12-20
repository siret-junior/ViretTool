using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    // TODO: sample just unfiltered part of the input ranking
    public class RankedDatasetFilter : FilterBase,
        IColorSignatureRankedDatasetFilter, IKeywordRankedDatasetFilter, ISemanticExampleRankedDatasetFilter
    {
        public IDatasetService DatasetService { get; }

        private const int SAMPLE_SIZE = 1000;
        private const int RANDOM_SEED = 42;
        private int[] _sampleIndexes;
        private double[] _sampleValues;

        // TODO: some other way of linking similarity modules to this filtering module
        //public RankingBuffer InputRanking { get; private set; }
        private RankingBuffer _inputRanking;
        public RankingBuffer InputRanking
        {
            get { return _inputRanking; }
            set
            {
                _inputRanking = value;

                // allocate filter mask
                base.IncludeMask = new bool[_inputRanking.Ranks.Length];

                // initialize sampling indexes
                if (_sampleIndexes == null)
                {
                    _sampleIndexes = new int[SAMPLE_SIZE];
                    _sampleValues = new double[SAMPLE_SIZE];

                    Random random = new Random(RANDOM_SEED);
                    for (int i = 0; i < SAMPLE_SIZE; i++)
                    {
                        _sampleIndexes[i] = random.Next(_inputRanking.Ranks.Length);
                    }
                }
            }
        }

        //public RankedDatasetFilter(IDatasetService datasetService) : base(new bool[datasetService.FrameCount])
        public RankedDatasetFilter() : base(null)
        {
            // initialize sampling indexes
            //Random random = new Random(RANDOM_SEED);
            //for (int i = 0; i < SAMPLE_SIZE; i++)
            //{
            //    _sampleIndexes[i] = random.Next(datasetService.FrameCount);
            //}
        }


        public void Include(double percentageOfDatabase)
        {
            // TODO: parallel for optimization for small loop bodies using range partitioner

            // estimate a rank value threshold for a given percentageOfDatabase
            Parallel.For(0, SAMPLE_SIZE, i =>
            {
                _sampleValues[i] = InputRanking.Ranks[_sampleIndexes[i]];

                // TODO: are we sampling in the entire dataset or just in the active subset (first half of dataset)
                if (_sampleValues[i] == float.MinValue)
                {
                    throw new NotImplementedException("RankedDatasetFilter sampling of a previously filtered frame.");
                }
            });
            
            Array.Sort(_sampleValues);
            double threshold = _sampleValues[(int)((SAMPLE_SIZE - 1) * percentageOfDatabase)];

            // set mask using the threshold
            Parallel.For(0, InputRanking.Ranks.Length, i =>
            {
                _includeMask[i] = InputRanking.Ranks[i] >= threshold;
                _excludeMask[i] = !_includeMask[i];
            });

            Mask = IncludeMask;
        }



        public bool[] GetFilterMask(ThresholdFilteringQuery query, RankingBuffer inputRanking)
        {
            InputRanking = inputRanking;

            switch (query.FilterState)
            {
                case ThresholdFilteringQuery.State.IncludeAboveThreshold:
                    Include((float)query.Threshold);
                    Mask = ExcludeMask;
                    return Mask;
                case ThresholdFilteringQuery.State.ExcludeAboveThreshold:
                    Include((float)query.Threshold);
                    return Mask;
                case ThresholdFilteringQuery.State.Off:
                    return null;
                default:
                    throw new NotImplementedException(
                        $"Filter state {Enum.GetName(typeof(FilterState), query.FilterState)} not expected.");
            }
        }

    }
}
