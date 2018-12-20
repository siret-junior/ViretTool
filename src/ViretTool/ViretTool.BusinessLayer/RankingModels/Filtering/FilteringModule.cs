using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering
{
    public class FilteringModule : IFilteringModule
    {
        // color filters
        public IColorSaturationFilter ColorSaturationFilter { get; }
        public IPercentOfBlackFilter PercentOfBlackColorFilter { get; }
        // ranking model percent of dataset filters
        public IColorSignatureRankedDatasetFilter ColorSignatureRankingFilter { get; }
        public IKeywordRankedDatasetFilter KeywordRankingFilter { get; }
        public ISemanticExampleRankedDatasetFilter SemanticExampleRankingFilter { get; }

        public FilteringQuery CachedQuery { get; set; }
        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }

        private bool[] _aggregatedFilterMask;


        public FilteringModule(
            IColorSaturationFilter colorSaturationFilter,
            IPercentOfBlackFilter percentOfBlackColorFilter,
            IColorSignatureRankedDatasetFilter colorSignatureRankingFilter,
            IKeywordRankedDatasetFilter keywordRankingFilter,
            ISemanticExampleRankedDatasetFilter semanticExampleRankingFilter)
        {
            ColorSaturationFilter = colorSaturationFilter;
            PercentOfBlackColorFilter = percentOfBlackColorFilter;
            ColorSignatureRankingFilter = colorSignatureRankingFilter;
            KeywordRankingFilter = keywordRankingFilter;
            SemanticExampleRankingFilter = semanticExampleRankingFilter;
        }


        public void ComputeRanking(FilteringQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking,
            RankingBuffer colorSignatureRanking, 
            RankingBuffer keywordRanking, 
            RankingBuffer semanticExampleRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;
            
            if ((query == null && CachedQuery == null) 
                || (query.Equals(CachedQuery) && !InputRanking.IsUpdated))
            {
                OutputRanking.IsUpdated = false;
                return;
            }
            OutputRanking.IsUpdated = true;

            // if not all filters are off
            if (query != null &&
                (query.ColorSaturationQuery.FilterState != ThresholdFilteringQuery.State.Off
                || query.PercentOfBlackQuery.FilterState != ThresholdFilteringQuery.State.Off
                //|| query.ColorSketchFilteringQuery.FilterState != ThresholdFilteringQuery.State.Off
                //|| query.KeywordFilteringQuery.FilterState != ThresholdFilteringQuery.State.Off
                //|| query.SemanticExampleFilteringQuery.FilterState != ThresholdFilteringQuery.State.Off
                ))
            {
                // TODO: filters
                bool[] colorSaturationMask = ColorSaturationFilter.GetFilterMask(query.ColorSaturationQuery);
                bool[] percentOfBlackMask = PercentOfBlackColorFilter.GetFilterMask(query.PercentOfBlackQuery);

                //bool[] colorSignatureRankingMask 
                //    = ColorSignatureRankingFilter.GetFilterMask(query.ColorSketchFilteringQuery, colorSignatureRanking);
                //bool[] keywordRankingMask 
                //    = KeywordRankingFilter.GetFilterMask(query.KeywordFilteringQuery, keywordRanking);
                //bool[] semanticExampleMask 
                //    = SemanticExampleRankingFilter.GetFilterMask(query.SemanticExampleFilteringQuery, semanticExampleRanking);


                // aggregate filters
                List<bool[]> masks = new List<bool[]>(5);
                if (colorSaturationMask != null) { masks.Add(colorSaturationMask); }
                if (percentOfBlackMask != null) { masks.Add(percentOfBlackMask); }
                //if (colorSignatureRankingMask != null) { masks.Add(colorSignatureRankingMask); }
                //if (keywordRankingMask != null) { masks.Add(keywordRankingMask); }
                //if (semanticExampleMask != null) { masks.Add(semanticExampleMask); }
                bool[] aggregatedMask = AggregateMasks(masks);

                // apply filters
                ApplyFilters(aggregatedMask, inputRanking, outputRanking);

                // cache query and result (result is cached in output ranking)
                CachedQuery = query;
            }
            else
            {
                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
            }
        }

        
        private bool[] AggregateMasks(List<bool[]> masks)
        {
            // check null input
            if (masks == null || masks.Count == 0)
            {
                throw new ArgumentException("Input masks are empty!");
            }

            // initialize result
            if (_aggregatedFilterMask == null || _aggregatedFilterMask.Length != masks[0].Length)
            {
                _aggregatedFilterMask = new bool[masks[0].Length];
            }
            
            // aggregate masks
            int masksCount = masks.Count;
            Parallel.For(0, _aggregatedFilterMask.Length, index =>
            {
                for (int iMask = 0; iMask < masksCount; iMask++)
                {
                    if (masks[iMask][index] == true)
                    {
                        _aggregatedFilterMask[index] = true;
                    }
                    else
                    {
                        _aggregatedFilterMask[index] = false;
                        break;
                    }
                }
            });

            return _aggregatedFilterMask;
        }

        private static void ApplyFilters(bool[] mask, RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            // TODO: optimize: just copy the input ranking and then rewrite filtered ranks

            Parallel.For(0, inputRanking.Ranks.Length, itemId =>
            {
                if (inputRanking.Ranks[itemId] == float.MinValue
                    || mask[itemId] == false)
                {
                    // ignore already filtered ranks
                    outputRanking.Ranks[itemId] = float.MinValue;
                    return;
                }
                else
                {
                    outputRanking.Ranks[itemId] = inputRanking.Ranks[itemId];
                }
            });
        }

    }
}
