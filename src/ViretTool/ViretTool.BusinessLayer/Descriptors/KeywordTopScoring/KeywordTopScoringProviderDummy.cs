using System.Collections.Generic;

namespace ViretTool.BusinessLayer.Descriptors.KeywordTopScoring
{
    internal class KeywordTopScoringProviderDummy : IKeywordTopScoringProvider
    {
        public int ScoringVectorSize => 0;
        public int ScoringCount => 0;

        public Dictionary<int, (int frameId, float score)[]> TopScorings { get; } = new Dictionary<int, (int frameId, float score)[]>();

        public (int frameId, float scoring)[] GetTopScoring(int synsetId)
        {
            return new (int frameId, float scoring)[0];
        }
    }
}
