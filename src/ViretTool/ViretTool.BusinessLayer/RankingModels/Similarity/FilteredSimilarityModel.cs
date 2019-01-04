
//using System;
//using ViretTool.BusinessLayer.RankingModels.Queries;
//using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

//namespace ViretTool.BusinessLayer.RankingModels.Similarity
//{
//    public class FilteredSimilarityModel<TQuery, TSimilarityModel> 
//        : IFilteredSimilarityModel<TQuery, TSimilarityModel>
//        where TQuery : IQuery
//        where TSimilarityModel : ISimilarityModel<TQuery>
//    {
//        public TSimilarityModel SimilarityModel { get; }

//        public FilteredRankingQuery<TQuery> CachedQuery { get; private set; }

//        public RankingBuffer InputRanking { get; private set; }
//        public RankingBuffer IntermediateRanking { get; private set; }
//        public RankingBuffer OutputRanking { get; private set; }

//        public void ComputeRanking(FilteredRankingQuery<TQuery> query, 
//            RankingBuffer inputRanking, RankingBuffer outputRanking)
//        {
//            InputRanking = inputRanking;
//            OutputRanking = outputRanking;
//            InitializeIntermediateBuffers();

//            if (!HasQueryOrInputChanged(query, inputRanking))
//            {
//                // nothing changed, OutputRanking contains cached data from previous computation
//                OutputRanking.IsUpdated = false;
//                return;
//            }
//            else
//            {
//                CachedQuery = query;
//                OutputRanking.IsUpdated = true;
//            }

//            if (IsQueryEmpty(query))
//            {
//                // no query, output is the same as input
//                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
//                return;
//            }

            

//            FormerSimilarityModel.ComputeRanking(query., inputRanking, FormerIntermediateRanking);

//            RankFusion.ComputeRanking(FormerIntermediateRanking, LatterIntermediateRanking, OutputRanking);
//        }










//        public IDatasetService DatasetService { get; }

//        private const int SAMPLE_SIZE = 1000;
//        private const int RANDOM_SEED = 42;
//        private int[] _sampleIndexes;
//        private double[] _sampleValues;

//        // TODO: some other way of linking similarity modules to this filtering module
//        //public RankingBuffer InputRanking { get; private set; }
//        private RankingBuffer _inputRanking;
//        public RankingBuffer InputRanking
//        {
//            get { return _inputRanking; }
//            set
//            {
//                _inputRanking = value;
//                if (value == null) return;

//                // allocate filter mask
//                if (base.IncludeMask == null || base.IncludeMask.Length != _inputRanking.Ranks.Length)
//                {
//                    base.IncludeMask = new bool[_inputRanking.Ranks.Length];
//                }

//                // initialize sampling indexes
//                if (_sampleIndexes == null)
//                {
//                    _sampleIndexes = new int[SAMPLE_SIZE];
//                    _sampleValues = new double[SAMPLE_SIZE];

//                    Random random = new Random(RANDOM_SEED);
//                    for (int i = 0; i < SAMPLE_SIZE; i++)
//                    {
//                        _sampleIndexes[i] = random.Next(_inputRanking.Ranks.Length);
//                    }
//                }
//            }
//        }

//        //public RankedDatasetFilter(IDatasetService datasetService) : base(new bool[datasetService.FrameCount])
//        public RankedDatasetFilter() : base(null)
//        {
//            // initialize sampling indexes
//            //Random random = new Random(RANDOM_SEED);
//            //for (int i = 0; i < SAMPLE_SIZE; i++)
//            //{
//            //    _sampleIndexes[i] = random.Next(datasetService.FrameCount);
//            //}
//        }


//        public void Include(double percentageOfDatabase)
//        {
//            // TODO: parallel for optimization for small loop bodies using range partitioner

//            // estimate a rank value threshold for a given percentageOfDatabase
//            Parallel.For(0, SAMPLE_SIZE, i =>
//            {
//                _sampleValues[i] = InputRanking.Ranks[_sampleIndexes[i]];

//                // TODO: are we sampling in the entire dataset or just in the active subset (first half of dataset)
//                if (_sampleValues[i] == float.MinValue)
//                {
//                    throw new NotImplementedException("RankedDatasetFilter sampling of a previously filtered frame.");
//                }
//            });

//            Array.Sort(_sampleValues, (a, b) => b.CompareTo(a));
//            double threshold = _sampleValues[(int)((SAMPLE_SIZE - 1) * percentageOfDatabase)];

//            // set mask using the threshold
//            Parallel.For(0, InputRanking.Ranks.Length, i =>
//            {
//                _includeMask[i] = InputRanking.Ranks[i] >= threshold;
//                _excludeMask[i] = !_includeMask[i];
//            });

//            Mask = IncludeMask;
//        }



//        public bool[] GetFilterMask(ThresholdFilteringQuery query, RankingBuffer inputRanking)
//        {
//            InputRanking = inputRanking;
//            if (inputRanking == null)
//            {
//                return null;
//            }

//            switch (query.FilterState)
//            {
//                case ThresholdFilteringQuery.State.IncludeAboveThreshold:
//                    Include((float)query.Threshold);
//                    Mask = ExcludeMask;
//                    return Mask;
//                case ThresholdFilteringQuery.State.ExcludeAboveThreshold:
//                    Include((float)query.Threshold);
//                    return Mask;
//                case ThresholdFilteringQuery.State.Off:
//                    return null;
//                default:
//                    throw new NotImplementedException(
//                        $"Filter state {Enum.GetName(typeof(FilterState), query.FilterState)} not expected.");
//            }
//        }

//    }
//}
