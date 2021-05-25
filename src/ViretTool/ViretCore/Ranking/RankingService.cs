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
        public readonly W2vvBowToVector W2vvBowToVector;
        public readonly W2vvTextToVectorRemote W2vvTextToVectorRemote;
        public readonly ContextAwareRanker ContextAwareRanker;

        public RankingService(W2vvBowToVector w2vvBowToVector, W2vvTextToVectorRemote w2vvTextToVectorRemote, ContextAwareRanker contextAwareRanker) 
        {
            W2vvBowToVector = w2vvBowToVector;
            W2vvTextToVectorRemote = w2vvTextToVectorRemote;
            ContextAwareRanker = contextAwareRanker;
        }

        public void PreloadQuery(string[] querySentences)
        {
            throw new NotImplementedException();
        }


        public List<VideoSegment> ComputeRankedResultSet(string[] querySentences)
        {
            // first pass through W2VV
            string[][] sentencesOfWords = querySentences.Select(sentence => sentence.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            float[][] queryVectors = sentencesOfWords.Select(sentenceWords => W2vvBowToVector.BowToVector(sentenceWords)).ToArray();
            // alternatively through remote W2VV service
            //float[][] queryVectors = sentencesOfWords.Select(sentenceWords => W2vvTextToVectorRemote.TextToVector(sentenceWords)).ToArray();

            // then through context-aware ranking
            int segmentSize = 10;
            List<VideoSegment> resultSet = ContextAwareRanker.RankVideoSegments(queryVectors, segmentSize);

            // order by score
            List<VideoSegment> rankedResultSet = resultSet
                .OrderByDescending(segment => segment.Score)
                .ThenByDescending(segment => segment.SegmentFirstFrameIndex)
                .ToList();

            return rankedResultSet;
        }
    }
}
