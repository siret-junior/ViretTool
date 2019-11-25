using System;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public abstract class FrameSynsetsIOBase : IDisposable
    {
        public const string KEYWORD_EXTENSION = ".framesynsets";

        public abstract void Dispose();
    }
}
