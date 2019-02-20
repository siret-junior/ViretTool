using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    public class ThumbnailReaderPreloaded : ThumbnailReader
    {
        private readonly ThumbnailRaw[] _thumbnailsPreloaded;

        public ThumbnailReaderPreloaded(string filePath) : base(filePath)
        {
            // preload thumbnails
            _thumbnailsPreloaded = new ThumbnailRaw[ThumbnailCount];
            for (int globalId = 0; globalId < ThumbnailCount; globalId++)
            {
                _thumbnailsPreloaded[globalId] = base.ReadVideoThumbnail(globalId);
            }
        }

        public override ThumbnailRaw ReadVideoThumbnail(int globalId)
        {
            return _thumbnailsPreloaded[globalId];
        }
    }
}
