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

        public void PreloadQuery(string[] querySentences)
        {
            throw new NotImplementedException();
        }


        public List<VideoSegment> ComputeRankedResultSet(string[] querySentences, RankingModel rankingModel = RankingModel.W2vvBow)
        {
            // split to sentences of words
            string[][] sentencesOfWords = querySentences.Select(sentence => sentence.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            // context-aware ranking based on the selected ranking model
            List<VideoSegment> resultSet;
            switch (rankingModel)
            {
                case RankingModel.W2vvBow:
                    {
                        float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.BowToVectorW2vv.BowToVector(sentenceWords)).ToArray();
                        resultSet = ViretCore.ContextAwareRankerW2vv.RankVideoSegments(queryVectors);
                    }
                    break;
                case RankingModel.W2vvBert:
                    {
                        // TODO: parallel?
                        float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.TextToVectorRemoteBert.TextToVector(sentenceWords)).ToArray();
                        resultSet = ViretCore.ContextAwareRankerBert.RankVideoSegments(queryVectors);
                    }
                    break;
                case RankingModel.Clip:
                    {
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
    }
}
