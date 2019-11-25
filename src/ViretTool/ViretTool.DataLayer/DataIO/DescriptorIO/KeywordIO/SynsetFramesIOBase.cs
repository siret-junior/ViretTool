using System;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public abstract class SynsetFramesIOBase : IDisposable
    {
        public const string SYNSET_FRAMES_EXTENSION = ".synsetframes";

        public abstract void Dispose();
    }
}
