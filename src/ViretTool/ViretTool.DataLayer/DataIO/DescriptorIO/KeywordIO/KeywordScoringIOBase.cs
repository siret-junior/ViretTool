using System;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public abstract class KeywordScoringIOBase : IDisposable
    {
        public const string KEYWORD_SCORING_EXTENSION = ".keyword";

        public abstract void Dispose();
    }
}
