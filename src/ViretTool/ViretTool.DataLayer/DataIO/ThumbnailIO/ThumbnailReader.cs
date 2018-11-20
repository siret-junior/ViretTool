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
        /// Total count of all thumbnails stored in the binary file.
        /// </summary>
        public int ThumbnailCount { get; }

        /// <summary>
        /// Width of stored thumbnail images.
        /// </summary>
        public int ThumbnailWidth { get; }

        /// <summary>
        /// Height of stored thumbnail images.
        /// </summary>
        public int ThumbnailHeight { get; }

        public int VideoCount { get; private set; }
        public int FrameCount { get; private set; }
        public int[] VideoGlobalIdOffsets { get; private set; }
        public int[] VideoGlobalIdLengths { get; private set; }

        private (int videoId, int frameNumber)[] _videoFrameTranslator;
        private Dictionary<(int videoId, int frameId), int> _globalIdTranslator
            = new Dictionary<(int videoId, int frameId), int>();
        
        
        public ThumbnailReader(string filePath)
        {
            BaseBlobReader = new VariableSizeBlobReader(filePath);
            ThumbnailCount = BaseBlobReader.BlobCount;

            byte[] metadata = BaseBlobReader.FiletypeMetadata;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            {
                ThumbnailWidth = reader.ReadInt32();
                ThumbnailHeight = reader.ReadInt32();

                VideoCount = reader.ReadInt32();
                FrameCount = reader.ReadInt32();

                VideoGlobalIdOffsets = DataConversionUtilities.TranslateToIntArray(
                    reader.ReadBytes(VideoCount * sizeof(int)));
                VideoGlobalIdLengths = DataConversionUtilities.TranslateToIntArray(
                    reader.ReadBytes(VideoCount * sizeof(int)));

                // load globalId <-> (videoId, frameId) mappings
                for (int iThumb = 0; iThumb < ThumbnailCount; iThumb++)
                {
                    int globalId = reader.ReadInt32();
                    int videoId = reader.ReadInt32();
                    int frameNumber = reader.ReadInt32();

                    _globalIdTranslator.Add((videoId, frameNumber), globalId);
                    _videoFrameTranslator[globalId] = (videoId, frameNumber);
                }
            }
        }

        public ThumbnailRaw[] ReadVideoThumbnails(int videoId)
        {
            int globalIdStart = VideoGlobalIdOffsets[videoId];
            int videoLength = VideoGlobalIdLengths[videoId];
            int globalIdEnd = globalIdStart + videoLength;
            ThumbnailRaw[] thumbnails = new ThumbnailRaw[videoLength];

            for (int globalId = globalIdStart; globalId < globalIdEnd; globalId++)
            {
                int frameNumber = _videoFrameTranslator[globalId].frameNumber;
                byte[] jpegData = BaseBlobReader.ReadByteBlob(globalId);

                thumbnails[globalId] = new ThumbnailRaw(videoId, frameNumber, jpegData);
            }

            return thumbnails;
        }

        public ThumbnailRaw ReadVideoThumbnail(int videoId, int frameNumber)
        {
            if (_globalIdTranslator.TryGetValue((videoId, frameNumber), out int globalId))
            {
                byte[] jpegData = BaseBlobReader.ReadByteBlob(globalId);
                return new ThumbnailRaw(videoId, frameNumber, jpegData);
            }
            else
            {
                // TODO: seek closest frame
                // (this should not happen though)
                throw new NotImplementedException();
            }
        }

        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }
    }
}
