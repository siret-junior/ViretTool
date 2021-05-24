using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Thumbnails
{
    public class ThumbnailJpeg
    {
        public int VideoId { get; }
        public int Frame { get; }
        public byte[] ImageJpeg { get; }

        public ThumbnailJpeg(int videoId, int frame, byte[] imageJpeg)
        {
            VideoId = videoId;
            Frame = frame;
            ImageJpeg = imageJpeg;
        }



        public override string ToString()
        {
            return $"Video {VideoId}, Frame {Frame}, JPEG length: {ImageJpeg.Length}";
        }
    }
}
