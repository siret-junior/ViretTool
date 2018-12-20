using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public interface IRankedDatasetFilter
    {
        RankingBuffer InputRanking { get; }
        void Include(double percentageOfDatabase);
        bool[] GetFilterMask(ThresholdFilteringQuery query, RankingBuffer inputRanking);
    }
}
