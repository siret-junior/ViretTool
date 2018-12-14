using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Fusion
{
    public class RankFusionWeightedProduct : IRankFusion
    {
        public Ranking OutputRanking { get; set; }

        public void ComputeRanking(Ranking[] rankings)
        {
            throw new NotImplementedException();
        }
    }
}
