using System;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO
{
    public abstract class ColorSignatureIOBase : IDisposable
    {
        public const string COLOR_SIGNATURES_EXTENSION = ".color";

        public abstract void Dispose();
    }
}
