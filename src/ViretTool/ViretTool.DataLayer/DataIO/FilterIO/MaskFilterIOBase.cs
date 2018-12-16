using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.FilterIO
{
    public abstract class MaskFilterIOBase : IDisposable
    {
        public const string MASK_FILTER_EXTENSION = ".maskfilter";
        public const string MASK_FILTER_FILETYPE_ID = "Mask filter";
        public const int MASK_FILTER_VERSION = 0;

        public const int METADATA_RESERVE_SPACE_SIZE = 1024 * 1024 * 10; // 10MB

        public abstract void Dispose();
    }
}
