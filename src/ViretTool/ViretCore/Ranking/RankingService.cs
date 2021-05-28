using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viret.DataModel;
using Viret.Ranking.ContextAware;
using Viret.Ranking.W2VV;

namespace Viret.Ranking
{
    /// <summary>
    /// Input: query
    /// Output: ranking
    /// </summary>
    public class RankingService
    {
        public readonly ViretCore ViretCore;
        public enum RankingModel { W2vvBow, W2vvBert, Clip }
        
        public RankingService(ViretCore viretCore) 
        {
            ViretCore = viretCore;
        }

        /// <summary>
        /// Computes ranked result set for the selected model.
        /// </summary>
        /// <param name="querySentences"></param>
        /// <param name="rankingModel"></param>
        /// <returns></returns>
        public List<VideoSegment> ComputeRankedResultSet(string[] querySentences, RankingModel rankingModel)
        {
            if (querySentences == null || querySentences.Length == 0) return new List<VideoSegment>();

            // split to sentences of words
            string[][] sentencesOfWords = querySentences.Select(sentence => sentence.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            // context-aware ranking based on the selected ranking model (TODO: consider implementing a common interface)
            List<VideoSegment> resultSet;
            switch (rankingModel)
            {
                case RankingModel.W2vvBow:
                    {
                        if (ViretCore.BowToVectorW2vv == null || ViretCore.ContextAwareRankerW2vv == null)
                        {
                            resultSet = new List<VideoSegment>();
                            break;
                        }
                        float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.BowToVectorW2vv.BowToVector(sentenceWords)).ToArray();
                        resultSet = ViretCore.ContextAwareRankerW2vv.RankVideoSegments(queryVectors);
                    }
                    break;
                case RankingModel.W2vvBert:
                    {
                        if (ViretCore.TextToVectorRemoteBert == null || ViretCore.ContextAwareRankerBert == null)
                        {
                            resultSet = new List<VideoSegment>();
                            break;
                        }
                        // TODO: parallel?
                        float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.TextToVectorRemoteBert.TextToVector(sentenceWords)).ToArray();
                        resultSet = ViretCore.ContextAwareRankerBert.RankVideoSegments(queryVectors);
                    }
                    break;
                case RankingModel.Clip:
                    {
                        if (ViretCore.TextToVectorRemoteClip == null || ViretCore.ContextAwareRankerClip == null)
                        {
                            resultSet = new List<VideoSegment>();
                            break;
                        }
                        // TODO: parallel?
                        float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.TextToVectorRemoteClip.TextToVector(sentenceWords)).ToArray();
                        resultSet = ViretCore.ContextAwareRankerClip.RankVideoSegments(queryVectors);
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unexpected ranking model: '{Enum.GetName(typeof(RankingModel), rankingModel)}'.");
            }

            // order by score
            List<VideoSegment> rankedResultSet = resultSet
                // prioritize score
                .OrderByDescending(segment => segment.Score)
                // then placement of the query close to center
                .ThenBy(segment => 
                    // distance between
                    Math.Abs(
                        // middle keyframeId
                        segment.SegmentFirstFrameIndex + (segment.Length - 1) / 2
                        // average keyframeId
                        - segment.KeyframeIdForEachQuery.Average()
                    )
                )
                // alternatively placement of the query from the beginning of the segment
                //.ThenByDescending(segment => segment.SegmentFirstFrameIndex)
                .ToList();

            return rankedResultSet;
        }


        /// <summary>
        /// Preloads queries for all models.
        /// </summary>
        /// <param name="querySentences"></param>
        public void PreloadQuery(string[] querySentences)
        {
            if (querySentences == null || querySentences.Length == 0) return;

            // split to sentences of words
            string[][] sentencesOfWords = querySentences.Select(sentence => sentence.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            if (ViretCore.BowToVectorW2vv != null && ViretCore.ContextAwareRankerW2vv != null)
            {
                Task.Run(() =>
                {
                    float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.BowToVectorW2vv.BowToVector(sentenceWords)).ToArray();
                    ViretCore.ContextAwareRankerW2vv.PreloadQueryConcurrent(queryVectors);
                });
            }

            if (ViretCore.TextToVectorRemoteBert != null && ViretCore.ContextAwareRankerBert != null)
            {
                Task.Run(() =>
                {
                    float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.TextToVectorRemoteBert.TextToVector(sentenceWords)).ToArray();
                    ViretCore.ContextAwareRankerBert.PreloadQueryConcurrent(queryVectors);
                });
            }

            if (ViretCore.TextToVectorRemoteClip != null && ViretCore.ContextAwareRankerClip != null)
            {
                Task.Run(() =>
                {
                    float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.TextToVectorRemoteClip.TextToVector(sentenceWords)).ToArray();
                    ViretCore.ContextAwareRankerClip.RankVideoSegments(queryVectors);
                });
            }
        }
    }
}
