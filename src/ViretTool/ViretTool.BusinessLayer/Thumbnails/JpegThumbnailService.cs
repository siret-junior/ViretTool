using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.ThumbnailIO;

namespace ViretTool.BusinessLayer.Thumbnails
{
    public class JpegThumbnailService : IThumbnailService<Thumbnail<byte[]>>
    {
        public const string FILE_EXTENSION = ".thumbnails";

        private readonly ThumbnailReader _baseThumbnailReader;


        public JpegThumbnailService(string datasetDirectory)
        {
            string[] files = Directory.GetFiles(datasetDirectory);
            string inputFile = files.Single(path => path.EndsWith(FILE_EXTENSION));

#if PRELOAD_THUMBNAILS
            _baseThumbnailReader = new ThumbnailReaderPreloaded(inputFile);
#else
            _baseThumbnailReader = new ThumbnailReader(inputFile);
#endif
        }


        public Thumbnail<byte[]> GetThumbnail(int videoId, int frameNumber)
        {
            ThumbnailDataJpeg thumbnailRaw = _baseThumbnailReader.ReadVideoThumbnail(videoId, frameNumber);
            return new Thumbnail<byte[]>(videoId, frameNumber, thumbnailRaw.JpegData);
        }

        public Thumbnail<byte[]>[] GetThumbnails(int videoId)
        {
            ThumbnailDataJpeg[] thumbnailsRaw = _baseThumbnailReader.ReadVideoThumbnails(videoId);
            Thumbnail<byte[]>[] thumbnails = new Thumbnail<byte[]>[thumbnailsRaw.Length];
            for (int i = 0; i < thumbnails.Length; i++)
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

        public void Dispose()
        {
            _baseThumbnailReader?.Dispose();
        }
    }
}
