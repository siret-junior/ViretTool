using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO
{
    public abstract class FloatVectorIOBase : IDisposable
    {
        public const string FLOAT_VECTOR_EXTENSION = ".floatvector";
        public const string FLOAT_VECTOR_FILETYPE_ID = "Float vector";
        public const int FLOAT_VECTOR_VERSION = 0;

        public const int METADATA_RESERVE_SPACE_SIZE = 1024 * 1024 * 10; // 10MB

        public abstract void Dispose();
    }
}
