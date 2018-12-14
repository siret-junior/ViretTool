using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO
{
    public abstract class ColorSignatureIOBase : IDisposable
    {
        public const string COLOR_SIGNATURES_EXTENSION = ".color";
        public const string COLOR_SIGNATURES_FILETYPE_ID = "Color signatures";
        public const int COLOR_SIGNATURES_VERSION = 0;

        public const int METADATA_RESERVE_SPACE_SIZE = 1024 * 1024 * 10; // 10MB

        public abstract void Dispose();
    }
}
