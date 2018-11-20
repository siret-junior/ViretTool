using System;

namespace ViretTool.DataLayer.DataIO.BlobIO.FixedSize
{
    public abstract class FizedSizeBlobIOBase : IDisposable
    {
        public const string FIXED_SIZE_BLOB_FILETYPE_ID = "Fixed-size blob";
        public const int FIXED_SIZE_BLOB_VERSION = 0;

        public abstract void Dispose();
    }
}
