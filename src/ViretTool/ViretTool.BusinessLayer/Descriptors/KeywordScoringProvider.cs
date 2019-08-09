using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO;

namespace ViretTool.BusinessLayer.Descriptors
{
    // TODO: consider generalizing scores similarly to descriptor providers
    // (provides precomputed score for a query)
    public class KeywordScoringProvider : IKeywordScoringProvider
    {
        public Dictionary<int, float[]> Scorings { get; }
        public Dictionary<int, (int frameId, float score)[]> TopScorings { get; }
        public int TopKScoreCount { get; private set; }

        public int ScoringVectorSize { get; private set; }
        public int ScoringCount { get; private set; }


        public KeywordScoringProvider(string inputFile, int topKScoreCount = 0)
        {
            using (KeywordScoringReader reader = new KeywordScoringReader(inputFile))
            {
                ScoringVectorSize = reader.ScoringVectorSize;
                ScoringCount = reader.ScoringCount;

                Scorings = new Dictionary<int, float[]>();
                for (int i = 0; i < ScoringCount; i++)
                {
                    int synsetId = reader.IdToSynsetIdMapping[i];
                    Scorings[synsetId] = reader.ReadScoring(i);
                }
            }

            // precompute top K scores per synset
            TopKScoreCount = topKScoreCount;
            TopScorings = new Dictionary<int, (int frameId, float score)[]>();
            foreach (int synsetId in Scorings.Keys)
            {
                TopScorings[synsetId] = Scorings[synsetId]
                    .Select((score, frameId) => (frameId, score))
                    .OrderByDescending(x => x.score)
                    .Take(TopKScoreCount)
                    .ToArray();
            }
        }

        public static KeywordScoringProvider FromDirectory(string directory, int topKScoreCount = 0)
        {
            string inputFile = Directory.GetFiles(directory)
                    .Where(dir => Path.GetFileName(dir).EndsWith(KeywordScoringIOBase.KEYWORD_SCORING_EXTENSION))
                    .FirstOrDefault();

            if (inputFile != null)
            {
                return new KeywordScoringProvider(inputFile, topKScoreCount);
            }
            else
            {
                return null;
            }
        }


        public float[] GetScoring(int scoringIndex)
        {
            return Scorings[scoringIndex];
        }

        public (int frameId, float scoring)[] GetTopScoring(int synsetId)
        {
            return TopScorings[synsetId];
        }
    }
}
