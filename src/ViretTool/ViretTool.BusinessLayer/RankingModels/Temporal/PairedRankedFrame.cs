using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public struct PairedRankedFrame
    {
        public int Id { get; }
        public float Rank { get; }
        public int PairId { get; }

        public PairedRankedFrame(int id, float rank, int pairId)
        {
            Id = id;
            Rank = rank;
            PairId = pairId;
        }
    }
}
