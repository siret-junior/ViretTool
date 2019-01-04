using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public interface ICountRestrictionFilter
    {
        IDatasetService DatasetService { get; }
    }
}
