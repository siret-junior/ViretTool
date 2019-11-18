using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class CountFilteringQuery : IEquatable<CountFilteringQuery>
    {
        public enum State { Enabled, Disabled }
        public State FilterState { get; set; }

        public int MaxPerVideo { get; set; }
        public int MaxPerShot { get; set; }
        public int MaxPerGroup { get; set; }


        public CountFilteringQuery(State filterState, int maxPerVideo, int maxPerShot, int maxPerGroup)
        {
            FilterState = filterState;
            MaxPerVideo = maxPerVideo;
            MaxPerShot = maxPerShot;
            MaxPerGroup = maxPerGroup;
        }


        public bool Equals(CountFilteringQuery other)
        {
            return FilterState.Equals(other.FilterState) &&
                MaxPerVideo.Equals(other.MaxPerVideo) &&
                MaxPerShot.Equals(other.MaxPerShot) &&
                MaxPerGroup.Equals(other.MaxPerGroup);
        }
    }
}
