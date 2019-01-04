using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO
{
    public abstract class BoolSignatureIOBase : IDisposable
    {
        public const string BOOL_SIGNATURES_FILETYPE_ID = "Bool signatures";
        public const int BOOL_SIGNATURES_VERSION = 0;

        public const int METADATA_RESERVE_SPACE_SIZE = 1024 * 1024 * 10; // 10MB

        public abstract void Dispose();
    }
}
