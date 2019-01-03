using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public interface IBiTemporalRankingModule
    {
        IBiTemporalSimilarityModule BiTemporalSimilarityModule { get; }
        IFilteringModule PrimaryFilteringModule { get; }
        IFilteringModule SecondaryFilteringModule { get; }

        TemporalQuery CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        BiTemporalRankingBuffer IntermediateRanking { get; }
        BiTemporalRankingBuffer OutputRanking { get; }

        void ComputeRanking(TemporalQuery query, RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking);
    }
}
