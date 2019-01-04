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
        public ICountRestrictionFilter CountRestrictionFilter { get; }

        public FilteringQuery CachedQuery { get; set; }
        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer MaskIntermediateRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }
        
        private bool[] _aggregatedFilterMask;


        public FilteringModule(
            IColorSaturationFilter colorSaturationFilter,
            IPercentOfBlackFilter percentOfBlackColorFilter,
            ICountRestrictionFilter countRestrictionFilter)
        {
            ColorSaturationFilter = colorSaturationFilter;
            PercentOfBlackColorFilter = percentOfBlackColorFilter;
            CountRestrictionFilter = countRestrictionFilter;
        }


        public void ComputeRanking(FilteringQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking)
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

            // mask filters
            bool[] colorSaturationMask = ColorSaturationFilter.GetFilterMask(query.ColorSaturationQuery);
            bool[] percentOfBlackMask = PercentOfBlackColorFilter.GetFilterMask(query.PercentOfBlackQuery);
            
            // aggregate filters
            List<bool[]> masks = new List<bool[]>(2);
            if (colorSaturationMask != null) { masks.Add(colorSaturationMask); }
            if (percentOfBlackMask != null) { masks.Add(percentOfBlackMask); }
            bool[] aggregatedMask = AggregateMasks(masks);

            // apply mask filters
            if (aggregatedMask != null)
            {
                ApplyMaskFilters(aggregatedMask, InputRanking, MaskIntermediateRanking);
                CountRestrictionFilter.ComputeFiltering(query.CountFilteringQuery,
                    MaskIntermediateRanking, OutputRanking);
            }
            else
            {
                // mask filters are not applied, we skip MaskIntermediateRanking
                CountRestrictionFilter.ComputeFiltering(query.CountFilteringQuery,
                    InputRanking, OutputRanking);
            }
        }

        private void InitializeIntermediateBuffers()
        {
            // create itermediate rankings if neccessary
            if (MaskIntermediateRanking == null
                || MaskIntermediateRanking.Ranks.Length != InputRanking.Ranks.Length)
            {
                MaskIntermediateRanking = RankingBuffer.Zeros(
                    "MaskIntermediateRanking", InputRanking.Ranks.Length);
            }
        }

        private bool HasQueryOrInputChanged(FilteringQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(FilteringQuery query)
        {
            return query == null ||
                (
                    query.ColorSaturationQuery.FilterState == ThresholdFilteringQuery.State.Off
                    && query.PercentOfBlackQuery.FilterState == ThresholdFilteringQuery.State.Off
                    && (query.CountFilteringQuery.FilterState == CountFilteringQuery.State.Disabled
                        || (query.CountFilteringQuery.MaxPerVideo <= 0
                            && query.CountFilteringQuery.MaxPerShot <= 0
                            && query.CountFilteringQuery.MaxPerGroup <= 0
                        )
                    )
                );
        }

        private bool[] AggregateMasks(List<bool[]> masks)
        {
            // check null input
            if (masks == null || masks.Count == 0)
            {
                return null;
                //throw new ArgumentException("Input masks are empty!");
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

        private static void ApplyMaskFilters(bool[] mask, RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            // TODO: optimize: just copy the input ranking and then rewrite filtered ranks?

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
