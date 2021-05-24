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
        public readonly ContextAwareRanker ContextAwareRanker;

        public RankingService(W2vvBowToVector w2vvBowToVector, ContextAwareRanker contextAwareRanker) 
        {
            W2vvBowToVector = w2vvBowToVector;
            ContextAwareRanker = contextAwareRanker;
        }

        public List<VideoSegment> ComputeRankedResultSet(string[] querySentences)
        {
            // first pass through W2VV
            string[][] sentencesOfWords = querySentences.Select(sentence => sentence.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            float[][] queryVectors = sentencesOfWords.Select(sentenceWords => W2vvBowToVector.BowToVector(sentenceWords)).ToArray();

            // then through context-aware ranking
            int segmentSize = 10;
            List<VideoSegment> resultSet = ContextAwareRanker.RankVideoSegments(queryVectors, segmentSize);

            // order by score
            List<VideoSegment> rankedResultSet = resultSet.OrderByDescending(segment => segment.Score).ToList();

            // annotate
            //List<AnnotatedVideoSegment> annotatedResultSet = resultSet.Select(segment => new AnnotatedVideoSegment(segment, sentences)).ToList();

            return rankedResultSet;
        }
    }
}
