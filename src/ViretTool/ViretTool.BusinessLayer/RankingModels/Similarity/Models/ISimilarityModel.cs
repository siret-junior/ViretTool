using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public interface ISimilarityModel<TQuery>
    {
        TQuery CachedQuery { get; }
        RankingBuffer InputRanking { get; }
        RankingBuffer OutputRanking { get; }

        void ComputeRanking(TQuery query, RankingBuffer InputRanking, RankingBuffer OutputRanking);
    }
}
