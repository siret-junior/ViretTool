using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalRankFusion
    {
        IDatasetServicesManager DatasetServicesManager { get; }

        RankingBuffer PrimaryInputRanking { get; }
        RankingBuffer SecondaryInputRanking { get; }
        BiTemporalRankingBuffer OutputRanking { get; }

        void ComputeRanking(
            RankingBuffer primaryInputRanking, RankingBuffer secondaryInputRanking, 
            BiTemporalRankingBuffer outputRanking);
    }
}
