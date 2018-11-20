using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels
{
    public interface IRankFusion
    {
        Ranking OutputRanking { get; }

        void ComputeRanking(Ranking[] rankings);
    }
}
