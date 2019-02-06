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
        
        public ThumbnailWriter(string outputFile, byte[] datasetHeader, 
            int thumbnailWidth, int thumbnailHeight, int videoCount, int thumbnailCount, int framesPerSecond,
            (int videoId, int frameNumber)[] globalIdToVideoFramenumber)
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

                ProcessVideoFramenumberMappings(globalIdToVideoFramenumber, 
                    out int[] videoOffsets, out int[] videoFrameCounts);
                for (int i = 0; i < videoOffsets.Length; i++)
                {
                    writer.Write(videoOffsets[i]);
                }
                for (int i = 0; i < videoFrameCounts.Length; i++)
                {
                    writer.Write(videoFrameCounts[i]);
                }

                for (int i = 0; i < globalIdToVideoFramenumber.Length; i++)
                {
                    writer.Write(i);
                    writer.Write(globalIdToVideoFramenumber[i].videoId);
                    writer.Write(globalIdToVideoFramenumber[i].frameNumber);
                }

                // reserve space
                writer.Write(new byte[METADATA_RESERVE_SPACE_SIZE]);

                thumbnailMetadata = metadataStream.ToArray();
            }

            BaseBlobWriter = new VariableSizeBlobWriter(
                outputFile, datasetHeader, thumbnailCount, thumbnailMetadata);
        }

        public override void Dispose()
        {
            BaseBlobWriter.Dispose();
        }
        
        public void WriteThumbnail(byte[] thumbnailData)
        {
            BaseBlobWriter.WriteBlob(thumbnailData);
        }


        private static void ProcessVideoFramenumberMappings((int videoId, int frameNumber)[] thumbnailKeys,
            out int[] outputVideoOffsets, out int[] outputVideoFrameCounts)
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

            outputVideoOffsets = videoOffsets.ToArray();
            outputVideoFrameCounts = videoFrameCounts.ToArray();
        }
    }
}
