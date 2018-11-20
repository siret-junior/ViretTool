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
        Ranking InputRanking { get; }
        Ranking OutputRanking { get; }

        void ComputeRanking(TQuery query);
    }
}
