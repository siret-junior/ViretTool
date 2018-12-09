using System;

namespace ViretTool.DataLayer.DataIO.BlobIO.VariableSize
{
    public abstract class VariableSizeBlobIOBase : IDisposable
    {
        public const string VARIABLE_SIZE_BLOBS_FILETYPE_ID = "Variable-size blobs";
        public const int VARIABLE_SIZE_BLOBS_VERSION = 0;

        public const int METADATA_RESERVE_SPACE_SIZE = 1024 * 1024 * 10; // 10MB


        public abstract void Dispose();
    }
}
