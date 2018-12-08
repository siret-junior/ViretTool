using System;

namespace ViretTool.DataLayer.DataIO.BlobIO.VariableSize
{
    public abstract class VariableSizeBlobIOBase : IDisposable
    {
        public const string VARIABLE_SIZE_BLOB_FILETYPE_ID = "Variable-size blob";
        public const int VARIABLE_SIZE_BLOB_VERSION = 0;

        public abstract void Dispose();
    }
}
