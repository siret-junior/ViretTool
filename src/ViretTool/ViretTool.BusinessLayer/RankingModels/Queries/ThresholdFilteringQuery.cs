using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class ThresholdFilteringQuery : IEquatable<ThresholdFilteringQuery>
    {
        public enum State { IncludeAboveThreshold, ExcludeAboveThreshold, Off }

        public State FilterState { get; private set; }
        public double Threshold { get; private set; }


        public ThresholdFilteringQuery(State filterState, double threshold)
        {
            FilterState = filterState;
            Threshold = threshold;
        }


        public bool Equals(ThresholdFilteringQuery other)
        {
            return FilterState.Equals(other.FilterState) &&
                Threshold.Equals(other.Threshold);
        }
    }
}
