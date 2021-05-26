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
            // first pass through W2VV
            string[][] sentencesOfWords = querySentences.Select(sentence => sentence.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            float[][] queryVectors = sentencesOfWords.Select(sentenceWords => ViretCore.BowToVectorW2vv.BowToVector(sentenceWords)).ToArray();
            // alternatively through remote W2VV service
            //float[][] queryVectors = sentencesOfWords.Select(sentenceWords => W2vvTextToVectorRemote.TextToVector(sentenceWords)).ToArray();

            // then through context-aware ranking
            int segmentSize = 10;
            List<VideoSegment> resultSet = ViretCore.ContextAwareRanker.RankVideoSegments(queryVectors, segmentSize);

            // order by score
            List<VideoSegment> rankedResultSet = resultSet
                .OrderByDescending(segment => segment.Score)
                .ThenByDescending(segment => segment.SegmentFirstFrameIndex)
                .ToList();

            return rankedResultSet;
        }
    }
}
