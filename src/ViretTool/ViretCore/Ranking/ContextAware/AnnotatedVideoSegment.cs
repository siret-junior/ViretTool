using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.ContextAware
{
    public class AnnotatedVideoSegment : VideoSegment
    {
        public readonly string[] Annotations;
        public readonly double[] Scores;

        public AnnotatedVideoSegment(int queryCount, int segmentFirstKeyframeIndex, int segmentSize)
            : base(queryCount, segmentFirstKeyframeIndex, segmentSize)
        { 
        }

        public AnnotatedVideoSegment(VideoSegment videoSegment, string[] query)
            : base(videoSegment)
        {
            Annotations = Enumerable.Repeat("", videoSegment.Length).ToArray();
            Scores = new double[videoSegment.Length];
            Score = videoSegment.Score;

            for(int iQuery = 0; iQuery < query.Length; iQuery++)
            {
                int keyframeId = videoSegment.KeyframeIdForEachQuery[iQuery];
                int indexInSegment = keyframeId - videoSegment.SegmentFirstFrameIndex;
                Annotations[indexInSegment] = query[iQuery];
                Scores[indexInSegment] = videoSegment.ScoresForEachQuery[iQuery];
            }
        }
    }
}
