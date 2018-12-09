﻿using System;

namespace ViretTool.DataLayer.DataIO.BlobIO.FixedSize
{
    public abstract class FizedSizeBlobIOBase : IDisposable
    {
        public const string FIXED_SIZE_BLOBS_FILETYPE_ID = "Fixed-size blobs";
        public const int FIXED_SIZE_BLOBS_VERSION = 0;

        public abstract void Dispose();
    }
}
