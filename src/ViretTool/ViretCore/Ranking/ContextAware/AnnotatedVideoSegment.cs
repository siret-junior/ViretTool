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
            : base(videoSegment.KeyframeIdForEachQuery.Length, videoSegment.SegmentFirstFrameIndex, videoSegment.Length)
        {
            Annotations = Enumerable.Repeat("", videoSegment.Length).ToArray();
            Scores = new double[videoSegment.Length];

            for(int iQuery = 0; iQuery < query.Length; iQuery++)
            {
                int keyframeId = videoSegment.KeyframeIdForEachQuery[iQuery];
                // TODO: why index out of bounds?
                // HERE: keyframeId = 0, videoSegment.SegmentFirstFrameIndex = 11 => indexInSegment = -11 =>> index out of bounds!!
                int indexInSegment = keyframeId - videoSegment.SegmentFirstFrameIndex;
                Annotations[indexInSegment] = query[iQuery];
                Scores[indexInSegment] = videoSegment.ScoresForEachQuery[iQuery];
            }
        }
    }
}
