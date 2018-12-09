﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    public abstract class ThumbnailIOBase : IDisposable
    {
        public const string THUMBNAILS_EXTENSION = ".thumb";
        public const string THUMBNAILS_FILETYPE_ID = "JPEG thumbnails";
        public const int THUMBNAILS_VERSION = 0;

        public abstract void Dispose();
    }
}