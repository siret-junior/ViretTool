using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataIO.FilterIO;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public class ThresholdFilter : FilterBase, IColorSaturationFilter, IPercentOfBlackFilter
    {
        private readonly float[] _frameAttributes;

        public float Threshold { get; private set; }


        public ThresholdFilter(float[] frameAttributes) : base(new bool[frameAttributes.Length])
        {
            _frameAttributes = frameAttributes;
        }

        public static ThresholdFilter FromDirectory(string inputDirectory, string extension)
        {
            string filterFilename = Directory.GetFiles(inputDirectory)
                .Where(file => file.EndsWith(extension)).First();
            float[] filterAttributes = MaskFilterReader.ReadFilter(filterFilename);
            ThresholdFilter thresholdFilter = new ThresholdFilter(filterAttributes);
            return thresholdFilter;
        }


        public void IncludeAbove(float threshold)
        {
            Threshold = threshold;
            
            // set mask using the threshold
            Parallel.For(0, _includeMask.Length, i =>
            {
                _includeMask[i] = _frameAttributes[i] > threshold;
                _excludeMask[i] = !_includeMask[i];
            });

            Mask = IncludeMask;
        }

        public void ExcludeAbove(float threshold)
        {
            Threshold = threshold;

            // set mask using the threshold
            Parallel.For(0, _includeMask.Length, i =>
            {
                _includeMask[i] = _frameAttributes[i] <= threshold;
                _excludeMask[i] = !_includeMask[i];
            });

            Mask = IncludeMask;
        }


        public bool[] GetFilterMask(ThresholdFilteringQuery query)
        {
            switch (query.FilterState)
            {
                case ThresholdFilteringQuery.State.IncludeAboveThreshold:
                    IncludeAbove((float)query.Threshold);
                    return Mask;
                case ThresholdFilteringQuery.State.ExcludeAboveThreshold:
                    ExcludeAbove((float)query.Threshold);
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
