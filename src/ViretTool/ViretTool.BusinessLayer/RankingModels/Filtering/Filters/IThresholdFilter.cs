using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public interface IThresholdFilter
    {
        float Threshold { get; }
        void IncludeAbove(float threshold);
        void ExcludeAbove(float threshold);
        bool[] GetFilterMask(ThresholdFilteringQuery query);
    }
}
