using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.ContextAware
{
    public class ContextAwareRanker
    {
        /// <summary>
        /// Features of all frames from all videos, expected ordering based on global ID !!!
        /// </summary>
        public readonly float[][] KeyframeVectors;

        /// <summary>
        /// List of all last frame IDs from all videos, used to split mAllFrames
        /// </summary>
        public readonly int[] VideoLastFrameIds;

        //private readonly ConcurrentDictionary<string, double[]> _queryScoreCache = new ConcurrentDictionary<string, double[]>();

        public ContextAwareRanker(float[][] keyframeVectors, int[] videoLastFrameIds)
        {
            VideoLastFrameIds = videoLastFrameIds;
            KeyframeVectors = keyframeVectors;

            // check data integrity
            foreach (int id in videoLastFrameIds)
            {
                if (id >= keyframeVectors.Length)
                {
                    throw new Exception("Video last frame ID " + id + " is larger than dataset!");
                }
            }
        }


        /// <summary>
        /// Computes scores for all segments of size = segmentSize, uses cosine similarity + 1
        /// </summary>
        /// <param name="queryVectors">List of normalized query feature vectors</param>
        /// <param name="segmentSize">Number of frames in one segment</param>
        public List<VideoSegment> RankVideoSegments(IList<float[]> queryVectors, int segmentSize = 10)
        {
            // computes query scores, but uses cache -> needs reset with new search!
            List<double[]> frameScoresForEachQuery = ComputeScoresForQueries(queryVectors);

            // ranks segments from all videos
            List<VideoSegment> segments = new List<VideoSegment>();
            int videoFirstFrameId = 0;
            foreach (int videoLastFrameId in VideoLastFrameIds)
            {
                segments.AddRange(RankSegmentsInVideo(frameScoresForEachQuery, segmentSize, videoFirstFrameId, videoLastFrameId + 1));
                videoFirstFrameId = videoLastFrameId + 1;
            }

            return segments;
        }


        /// <summary>
        /// Preloads query and stores it in cache
        /// </summary>
        public void PreloadQueryConcurrent(IList<float[]> queryVectors)
        {
            ComputeScoresForQueries(queryVectors);
        }


        private IList<VideoSegment> RankSegmentsInVideo(
            List<double[]> scoresForEachQuery,
            int segmentSize,
            int videoStart,
            int videoEnd)
        {
            // for too short video, trim segmentSize
            if (videoEnd - videoStart < segmentSize)
            {
                segmentSize = videoEnd - videoStart;
            }

            //List<VideoSegment> segments = new List<VideoSegment>();
            int queryCount = scoresForEachQuery.Count;

            // scan one video with a window of size = segmentSize
            VideoSegment[] segments = new VideoSegment[videoEnd - segmentSize + 1 - videoStart];

            Parallel.For(videoStart, videoEnd - segmentSize + 1, segmentFirstKeyframeId =>
            {
                segments[segmentFirstKeyframeId - videoStart] = new VideoSegment(queryCount, segmentFirstKeyframeId, segmentSize);
                VideoSegment videoSegment = segments[segmentFirstKeyframeId - videoStart];

                // for one segment, find the best score and its position for each query
                int segmentEndExclusive = segmentFirstKeyframeId + segmentSize;
                for (int iQuery = 0; iQuery < queryCount; iQuery++)
                {
                    double[] queryScores = scoresForEachQuery[iQuery];
                    double max = double.MinValue;
                    int maxId = 0;

                    for (int keyframeId = segmentFirstKeyframeId; keyframeId < segmentEndExclusive; keyframeId++)
                    {
                        if (queryScores[keyframeId] > max)
                        {
                            max = queryScores[keyframeId];
                            maxId = keyframeId;
                        }
                    }

                    videoSegment.KeyframeIdForEachQuery[iQuery] = maxId;
                    videoSegment.ScoresForEachQuery[iQuery] = max;
                }

                videoSegment.UpdateSegmentScore();
            });

            return segments;
        }

        //private List<VideoSegment> GenerateVideoSegments(int segmentSize = 10)
        //{
        //    List<VideoSegment> segments = new List<VideoSegment>();
        //    int videoFirstFrameId = 0;
        //    foreach (int videoLastFrameId in VideoLastFrameIds)
        //    {
        //        segments.AddRange(GenerateSegmentsForVideo(segmentSize, videoFirstFrameId, videoLastFrameId + 1));
        //        videoFirstFrameId = videoLastFrameId + 1;
        //    }

        //    return segments;
        //}

        //private IEnumerable<VideoSegment> GenerateSegmentsForVideo(int segmentSize, int videoStart, int videoEnd)
        //{
        //    // for too short video, trim segmentSize
        //    if (videoEnd - videoStart < segmentSize)
        //    {
        //        segmentSize = videoEnd - videoStart;
        //    }

        //    // scan one video with a window of size = segmentSize
        //    for (int i = videoStart; i < videoEnd - segmentSize + 1; i++)
        //    {
        //        yield return new VideoSegment(10, i, segmentSize);
        //    }
        //}

        private List<double[]> ComputeScoresForQueries(IList<float[]> queryVectors)
        {
            List<double[]> scoreLists = new List<double[]>();

            foreach (float[] queryVector in queryVectors)
            {
                //string key = string.Join(",", queryVector);
                //if (!_queryScoreCache.TryGetValue(key, out double[] scores))
                //{
                //    scores = new double[KeyframeVectors.Length];
                //    Parallel.For(0, KeyframeVectors.Length, i => { scores[i] = DotProductPlusOne(queryVector, KeyframeVectors[i]); });
                //    _queryScoreCache.TryAdd(key, scores);
                //}
                double[] scores = new double[KeyframeVectors.Length];
                Parallel.For(0, KeyframeVectors.Length, i => { scores[i] = DotProductPlusOne(queryVector, KeyframeVectors[i]); });

                scoreLists.Add(scores);
            }

            return scoreLists;
        }



        /// <summary>
        /// Returns values from 0 to 2, but only for normalized vectors!
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private double DotProductPlusOne(float[] x, float[] y)
        {
            double result = 0;
            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }
            return result + 1;
        }
    }
}
