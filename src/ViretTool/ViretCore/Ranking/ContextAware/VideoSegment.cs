using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.ContextAware
{
    public class VideoSegment
    {
        /// <summary>
        /// List of global frame IDs that were matched in the segment for a list of queries 
        /// </summary>
        public readonly int[] KeyframeIdForEachQuery;

        /// <summary>
        /// List of scores for matched frames
        /// </summary>
        public readonly double[] ScoresForEachQuery;

        public readonly int SegmentFirstFrameIndex;
        public readonly int Length;
        

        /// <summary>
        /// Overall score of the segment, higher score is better
        /// </summary>
        public double Score = 1;

        public VideoSegment(int queryCount, int segmentFirstKeyframeIndex, int segmentSize)
        {
            KeyframeIdForEachQuery = new int[queryCount];
            ScoresForEachQuery = new double[queryCount];
            SegmentFirstFrameIndex = segmentFirstKeyframeIndex;
            Length = segmentSize;
        }

        public void ComputeSegmentScore()
        {
            // TODO - in future, test various aggregations...

            Score = 1;
            for (int i = 0; i < ScoresForEachQuery.Length; i++)
            {
                Score *= ScoresForEachQuery[i];
            }
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", Enumerable.Range(SegmentFirstFrameIndex, Length))}]";
        }
    }
}
