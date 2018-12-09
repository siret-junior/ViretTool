using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.ThumbnailIO;
using ViretTool.DataLayer.DataProviders.Thumbnails;

namespace ViretTool.BusinessLayer.Thumbnails
{
    public class JpegThumbnailService : IThumbnailService<Thumbnail<byte[]>>
    {
        private readonly ThumbnailReader _baseThumbnailReader;

        public JpegThumbnailService(ThumbnailProvider thumbnailProvider, string datasetFolder)
        {
            _baseThumbnailReader = thumbnailProvider.FromDirectory(datasetFolder);
        }


        public Thumbnail<byte[]> GetThumbnail(int videoId, int frameNumber)
        {
            ThumbnailRaw thumbnailRaw = _baseThumbnailReader.ReadVideoThumbnail(videoId, frameNumber);
            return new Thumbnail<byte[]>(videoId, frameNumber, thumbnailRaw.JpegData);
        }

        public Thumbnail<byte[]>[] GetThumbnails(int videoId)
        {
            ThumbnailRaw[] thumbnailsRaw = _baseThumbnailReader.ReadVideoThumbnails(videoId);
            Thumbnail<byte[]>[] thumbnails = new Thumbnail<byte[]>[thumbnailsRaw.Length];
            for (int i = 0; i < thumbnailsRaw.Length; i++)
            {
                thumbnails[i] = new Thumbnail<byte[]>(
                    thumbnailsRaw[i].VideoId,
                    thumbnailsRaw[i].FrameNumber,
                    thumbnailsRaw[i].JpegData);
            }
            return thumbnails;
        }

        public Thumbnail<byte[]>[] GetThumbnails(int videoId, int startFrame, int endFrame)
        {
            throw new NotImplementedException();
        }
    }
}
