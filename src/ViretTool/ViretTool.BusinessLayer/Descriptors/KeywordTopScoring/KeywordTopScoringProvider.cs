using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO;

namespace ViretTool.BusinessLayer.Descriptors.KeywordTopScoring
{
    // TODO: consider generalizing scores similarly to descriptor providers
    // (provides precomputed score for a query)
    public class KeywordTopScoringProvider : IKeywordTopScoringProvider
    {
        public Dictionary<int, (int frameId, float score)[]> TopScorings { get; }

        public int ScoringVectorSize { get; private set; }
        public int ScoringCount { get; private set; }


        public KeywordTopScoringProvider(Dictionary<int, (int frameId, float score)[]> topScorings,
            int scoringVectorSize, int scoringCount)
        {
            TopScorings = topScorings;
            ScoringVectorSize = scoringVectorSize;
            ScoringCount = scoringCount;
        }


        public (int frameId, float scoring)[] GetTopScoring(int synsetId)
        {
            (int frameId, float scoring)[] result;
            try
            {
                TopScorings.TryGetValue(synsetId, out result);
            }
            catch
            {
                throw new ArgumentNullException($"No keyword scoring found for synsetId {synsetId}");
                //return new (int frameId, float scoring)[0];
            }
            return result;
        }
    }
}
