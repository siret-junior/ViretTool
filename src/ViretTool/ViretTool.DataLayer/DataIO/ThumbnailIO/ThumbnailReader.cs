using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.VariableSize;

namespace ViretTool.DataLayer.DataIO.ThumbnailIO
{
    public class ThumbnailReader : ThumbnailIOBase
    {
        public VariableSizeBlobReader BaseBlobReader { get; private set; }
        public byte[] DatasetHeader => BaseBlobReader.DatasetHeader;
        
        /// <summary>
        /// Width of stored thumbnail images.
        /// </summary>
        public int ThumbnailWidth { get; }

        /// <summary>
        /// Height of stored thumbnail images.
        /// </summary>
        public int ThumbnailHeight { get; }

        public int VideoCount { get; private set; }
        public int ThumbnailCount => BaseBlobReader.BlobCount;
        public int FramesPerSecond { get; private set; }


        public (int videoId, int frameNumber)[] GlobalIdToVideoFramenumber { get; private set; }
        public int[] VideoOffsets { get; private set; }
        public int[] VideoFrameCounts { get; private set; }

        private Dictionary<int, Dictionary<int,int>> _videoFramenumberToGlobalId;
        
        
        public ThumbnailReader(string filePath)
        {
            BaseBlobReader = new VariableSizeBlobReader(filePath);

            byte[] metadata = BaseBlobReader.FiletypeMetadata;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            {
                ReadAndVerifyFiletypeAndVersion(reader);

                ThumbnailWidth = reader.ReadInt32();
                ThumbnailHeight = reader.ReadInt32();

                VideoCount = reader.ReadInt32();
                int thumbnailCount = reader.ReadInt32();
                if (thumbnailCount != ThumbnailCount)
                {
                    throw new IOException(
                        $"Thumbnail count mismatch between ThumbnailReader ({thumbnailCount})" +
                        $" and underlying BlobReader ({ThumbnailCount})");
                }
                FramesPerSecond = reader.ReadInt32();

                VideoOffsets = DataConversionUtilities.TranslateToIntArray(
                    reader.ReadBytes(VideoCount * sizeof(int)));
                VideoFrameCounts = DataConversionUtilities.TranslateToIntArray(
                    reader.ReadBytes(VideoCount * sizeof(int)));

                // load globalId <-> (videoId, frameId) mappings
                GlobalIdToVideoFramenumber = new (int videoId, int frameNumber)[ThumbnailCount];
                _videoFramenumberToGlobalId = new Dictionary<int, Dictionary<int, int>>();
                for (int iThumb = 0; iThumb < ThumbnailCount; iThumb++)
                {
                    int globalId = reader.ReadInt32();
                    int videoId = reader.ReadInt32();
                    int frameNumber = reader.ReadInt32();

                    if (!_videoFramenumberToGlobalId.ContainsKey(videoId))
                    {
                        _videoFramenumberToGlobalId.Add(videoId, new Dictionary<int, int>());
                    }

                    _videoFramenumberToGlobalId[videoId].Add(frameNumber, globalId);
                    GlobalIdToVideoFramenumber[globalId] = (videoId, frameNumber);
                }
            }
        }

        private void ReadAndVerifyFiletypeAndVersion(BinaryReader reader)
        {
            string filetype = reader.ReadString();
            if (!filetype.Equals(THUMBNAILS_FILETYPE_ID))
            {
                throw new IOException($"Filetype error: {filetype} (expected {THUMBNAILS_FILETYPE_ID})");
            }

            int version = reader.ReadInt32();
            if (version != THUMBNAILS_VERSION)
            {
                throw new IOException($"Incorrect \"{THUMBNAILS_FILETYPE_ID}\" filetype version: "
                    + $"{version} (expected {THUMBNAILS_VERSION})");
            }
        }

        public ThumbnailRaw[] ReadVideoThumbnails(int videoId)
        {
            int globalIdStart = VideoOffsets[videoId];
            int videoLength = VideoFrameCounts[videoId];
            int globalIdEnd = globalIdStart + videoLength;
            ThumbnailRaw[] thumbnails = new ThumbnailRaw[videoLength];

            for (int globalId = globalIdStart; globalId < globalIdEnd; globalId++)
            {
                int frameNumber = GlobalIdToVideoFramenumber[globalId].frameNumber;
                byte[] jpegData = BaseBlobReader.ReadByteBlob(globalId);

                thumbnails[globalId - globalIdStart] = new ThumbnailRaw(videoId, frameNumber, jpegData);
            }

            return thumbnails;
        }

        public ThumbnailRaw ReadVideoThumbnail(int videoId, int frameNumber)
        {
            if (_videoFramenumberToGlobalId.TryGetValue(videoId, out Dictionary<int, int> dict)
                && dict.TryGetValue(frameNumber, out int globalId))
            {
                return ReadVideoThumbnail(globalId);
            }
            else
            {
                // TODO: seek closest frame (this should not happen though)
                //int firstFrameGlobalId = _videoFramenumberToGlobalId[videoId][0];
                //for (int i = 0; i < VideoFrameCounts[videoId]; i++)
                //{
                //    if (GlobalIdToVideoFramenumber[firstFrameGlobalId + i].frameNumber >= frameNumber)
                //    {
                //        return ReadVideoThumbnail(firstFrameGlobalId + i);
                //    }
                //}

                throw new ArgumentException($"Trying to read thumbnail that is not stored [video: {videoId}, frame: {frameNumber}].");
            }
        }

        public virtual ThumbnailRaw ReadVideoThumbnail(int globalId)
        {
            byte[] jpegData = BaseBlobReader.ReadByteBlob(globalId);
            int videoId = GlobalIdToVideoFramenumber[globalId].videoId;
            int frameNumber = GlobalIdToVideoFramenumber[globalId].frameNumber;
            return new ThumbnailRaw(videoId, frameNumber, jpegData);
        }

        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }
    }
}
