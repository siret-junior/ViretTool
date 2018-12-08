namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class ThresholdFilteringQuery
    {
        public enum State { FilterBelowThreshold, FilterAboveThreshold, Off }

        public State FilterState { get; private set; }
        public double Threshold { get; private set; }


        public ThresholdFilteringQuery(State filterState, double threshold)
        {
            FilterState = filterState;
            Threshold = threshold;
        }

        public override bool Equals(object obj)
        {
            return obj is ThresholdFilteringQuery query &&
                   FilterState == query.FilterState &&
                   Threshold == query.Threshold;
        }

        public override int GetHashCode()
        {
            int hashCode = -1488501104;
            hashCode = hashCode * -1521134295 + FilterState.GetHashCode();
            hashCode = hashCode * -1521134295 + Threshold.GetHashCode();
            return hashCode;
        }
    }
}
