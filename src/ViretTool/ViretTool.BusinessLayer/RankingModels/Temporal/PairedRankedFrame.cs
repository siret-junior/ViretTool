using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public struct PairedRankedFrame : IComparable<PairedRankedFrame>
    {
        public int Id { get; }
        public float Rank { get; }
        public int PairId { get; }

        // TODO: rename ranked->scored
        public PairedRankedFrame(int id, float rank, int pairId)
        {
            Id = id;
            Rank = rank;
            PairId = pairId;
        }
        
        public int CompareTo(PairedRankedFrame other)
        {
            return Rank.CompareTo(other.Rank);
        }
    }
}
