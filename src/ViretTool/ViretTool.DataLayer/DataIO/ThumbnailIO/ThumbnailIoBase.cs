using System;

namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    public abstract class ThumbnailIOBase : IDisposable
    {
        public const string THUMBNAILS_EXTENSION = ".thumbnails";

        public abstract void Dispose();
    }
}
