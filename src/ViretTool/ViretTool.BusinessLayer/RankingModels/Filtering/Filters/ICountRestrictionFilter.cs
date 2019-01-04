using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public interface ICountRestrictionFilter
    {
        IDatasetService DatasetService { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeFiltering(CountFilteringQuery query,
            RankingBuffer inputRanking, RankingBuffer outputRanking);
    }
}
