using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class CountFilteringQuery
    {
        public enum State { Enabled, Disabled }

        public State FilterState { get; private set; }

        public int MaxPerVideo { get; private set; }
        public int MaxPerShot { get; private set; }
        public int MaxPerGroup { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is CountFilteringQuery query &&
                   FilterState == query.FilterState &&
                   MaxPerVideo == query.MaxPerVideo &&
                   MaxPerShot == query.MaxPerShot &&
                   MaxPerGroup == query.MaxPerGroup;
        }

        public override int GetHashCode()
        {
            int hashCode = -622218634;
            hashCode = hashCode * -1521134295 + FilterState.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxPerVideo.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxPerShot.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxPerGroup.GetHashCode();
            return hashCode;
        }
    }
}
