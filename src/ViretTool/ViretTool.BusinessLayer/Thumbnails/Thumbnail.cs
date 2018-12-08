using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Thumbnails
{
    public class Thumbnail<T>
    {
        public int VideoId { get; private set; }
        public int FrameNumber { get; private set; }
        public T Image { get; private set; }


        public Thumbnail(int videoId, int frameNumber, T image)
        {
            VideoId = videoId;
            FrameNumber = frameNumber;
            Image = image;
        }
    }
}
