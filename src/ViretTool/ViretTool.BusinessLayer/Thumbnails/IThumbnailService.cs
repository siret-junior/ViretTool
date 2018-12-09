using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Thumbnails
{
    public interface IThumbnailService<T>
    {
        T GetThumbnail(int videoId, int frameId);
        T[] GetThumbnails(int videoId);
        T[] GetThumbnails(int videoId, int startFrame, int endFrame);
    }
}
