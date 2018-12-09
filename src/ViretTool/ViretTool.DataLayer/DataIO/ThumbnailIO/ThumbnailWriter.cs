using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.VariableSize;

namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    public class ThumbnailWriter : ThumbnailIOBase
    {
        public VariableSizeBlobWriter BaseBlobWriter { get; private set; }
        public byte[] DatasetHeader => BaseBlobWriter.DatasetHeader;

        public int[] VideoOffsets { get; private set; }
        public int[] VideoFrameCounts { get; private set; }


        public ThumbnailWriter(string outputFile, byte[] datasetHeader, 
            int thumbnailWidth, int thumbnailHeight, int videoCount, int thumbnailCount, int framesPerSecond,
            (int videoId, int frameNumber)[] thumbnailKeys)
        {
            byte[] thumbnailMetadata = null;
            using (MemoryStream metadataStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(metadataStream))
            {
                writer.Write(THUMBNAILS_FILETYPE_ID);
                writer.Write(THUMBNAILS_VERSION);

                writer.Write(thumbnailWidth);
                writer.Write(thumbnailHeight);

                writer.Write(videoCount);
                writer.Write(thumbnailCount);
                writer.Write(framesPerSecond);

                ProcessThumbnailKeys(thumbnailKeys);
                for (int i = 0; i < VideoOffsets.Length; i++)
                {
                    writer.Write(VideoOffsets[i]);
                }
                for (int i = 0; i < VideoFrameCounts.Length; i++)
                {
                    writer.Write(VideoFrameCounts[i]);
                }

                for (int i = 0; i < thumbnailKeys.Length; i++)
                {
                    writer.Write(i);
                    writer.Write(thumbnailKeys[i].videoId);
                    writer.Write(thumbnailKeys[i].frameNumber);
                }

                // reserve space
                writer.Seek(METADATA_RESERVE_SPACE_SIZE, SeekOrigin.Current);

                thumbnailMetadata = metadataStream.ToArray();
            }

            BaseBlobWriter = new VariableSizeBlobWriter(
                outputFile, datasetHeader, thumbnailCount, thumbnailMetadata);
        }

        private void ProcessThumbnailKeys((int videoId, int frameNumber)[] thumbnailKeys)
        {
            List<int> videoOffsets = new List<int>();
            List<int> videoFrameCounts = new List<int>();

            int previousVideoId = -1;
            int frameCounter = 0;
            for (int i = 0; i < thumbnailKeys.Length; i++)
            {
                int videoId = thumbnailKeys[i].videoId;
                frameCounter++;

                if (previousVideoId != videoId)     // when starting a new video
                {
                    videoOffsets.Add(i);      // mark offset of the new video
                    if (previousVideoId != -1)
                    {
                        videoFrameCounts.Add(frameCounter);   // finish counting previous video frames
                        frameCounter = 0;
                    }
                    previousVideoId = videoId;
                }
            }
            videoFrameCounts.Add(frameCounter);   // finish the last video

            VideoOffsets = videoOffsets.ToArray();
            VideoFrameCounts = videoFrameCounts.ToArray();
        }

        public void WriteThumbnail(byte[] thumbnailData)
        {
            BaseBlobWriter.WriteBlob(thumbnailData);
        }



        public override void Dispose()
        {
            BaseBlobWriter.Dispose();
        }
    }
}
