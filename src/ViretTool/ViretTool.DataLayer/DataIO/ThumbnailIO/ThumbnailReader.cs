﻿using System;
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

        private readonly Dictionary<int, Dictionary<int,int>> _videoFramenumberToGlobalId;

        private const int RESOLUTION_LIMIT = 7680; // 8K UHD
        private const int VIDEOCOUNT_LIMIT = 100_000;
        private const int FRAMECOUNT_LIMIT = 1_000_000;
        private const int FPS_LIMIT = 30;

        public ThumbnailReader(string filePath)
        {
            BaseBlobReader = new VariableSizeBlobReader(filePath);

            byte[] metadata = BaseBlobReader.FiletypeMetadata;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            {
                //ReadAndVerifyFiletypeAndVersion(reader);

                ThumbnailWidth = reader.ReadInt32();
                ThumbnailHeight = reader.ReadInt32();
                CheckThumbnailResolution(ThumbnailWidth, ThumbnailHeight);

                VideoCount = reader.ReadInt32();
                FileFormatUtilities.CheckValueInRange("VideoCount", VideoCount, 1, VIDEOCOUNT_LIMIT);
                //int thumbnailCount = reader.ReadInt32();
                //if (thumbnailCount != BaseBlobReader.BlobCount)
                //{
                //    throw new IOException(
                //        $"Thumbnail count mismatch between ThumbnailReader ({thumbnailCount})" +
                //        $" and underlying BlobReader ({BaseBlobReader.BlobCount})");
                //}
                FramesPerSecond = reader.ReadInt32();
                FileFormatUtilities.CheckValueInRange("FramesPerSecond", FramesPerSecond, 1, FPS_LIMIT);

                VideoOffsets = DataConversionUtilities.ConvertToIntArray(
                    reader.ReadBytes(VideoCount * sizeof(int)));
                FileFormatUtilities.CheckValuesInRange("VideoOffsets", VideoOffsets, 0, ThumbnailCount - 1);
                FileFormatUtilities.CheckValuesIncrement("VideoOffsets", VideoOffsets);

                VideoFrameCounts = DataConversionUtilities.ConvertToIntArray(
                    reader.ReadBytes(VideoCount * sizeof(int)));
                FileFormatUtilities.CheckValuesInRange("VideoFrameCounts", VideoFrameCounts, 1, ThumbnailCount);
                
                // load globalId <-> (videoId, frameId) mappings
                GlobalIdToVideoFramenumber = new (int videoId, int frameNumber)[ThumbnailCount];
                _videoFramenumberToGlobalId = new Dictionary<int, Dictionary<int, int>>();
                
                for (int iThumb = 0; iThumb < ThumbnailCount; iThumb++)
                {
                    int globalId = iThumb;// reader.ReadInt32(); // TODO: remove? should be equal to iThumb
                    int videoId = reader.ReadInt32();
                    int frameNumber = reader.ReadInt32();
                    //FileFormatUtilities.CheckValueInRange("globalId", globalId, 0, ThumbnailCount - 1);
                    FileFormatUtilities.CheckValueInRange("videoId", videoId, 0, VideoCount - 1);
                    FileFormatUtilities.CheckValueInRange("frameNumber", frameNumber, 0, FRAMECOUNT_LIMIT);

                    if (!_videoFramenumberToGlobalId.ContainsKey(videoId))
                    {
                        _videoFramenumberToGlobalId.Add(videoId, new Dictionary<int, int>());
                    }

                    _videoFramenumberToGlobalId[videoId].Add(frameNumber, globalId);
                    GlobalIdToVideoFramenumber[globalId] = (videoId, frameNumber);
                }
            }
        }

        private void CheckThumbnailResolution(int width, int height)
        {
            FileFormatUtilities.CheckValueInRange("Thumbnail width", width, 1, RESOLUTION_LIMIT);
            FileFormatUtilities.CheckValueInRange("Thumbnail height", height, 1, RESOLUTION_LIMIT);
            
            if (width < height)
            {
                throw new InvalidDataException($"Thumbnail orientation {width} x {height} is expected to be in landscape (width > height).");
            }
        }

        //private void ReadAndVerifyFiletypeAndVersion(BinaryReader reader)
        //{
        //    string filetype = reader.ReadString();
        //    if (!filetype.Equals(THUMBNAILS_FILETYPE_ID))
        //    {
        //        throw new IOException($"Filetype error: {filetype} (expected {THUMBNAILS_FILETYPE_ID})");
        //    }

        //    int version = reader.ReadInt32();
        //    if (version != THUMBNAILS_VERSION)
        //    {
        //        throw new IOException($"Incorrect \"{THUMBNAILS_FILETYPE_ID}\" filetype version: "
        //            + $"{version} (expected {THUMBNAILS_VERSION})");
        //    }
        //}

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

                // TODO: return an error image instead of raising an exception
                //throw new ArgumentException($"Trying to read thumbnail that is not stored [video: {videoId}, frame: {frameNumber}].");
                return new ThumbnailRaw(videoId, frameNumber, ERROR_THUMBNAIL);
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

        private readonly byte[] ERROR_THUMBNAIL = new byte[] 
        {
            0xff, 0xd8, 0xff, 0xe0, 0x00, 0x10, 0x4a, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0xf0,
            0x00, 0xf0, 0x00, 0x00, 0xff, 0xdb, 0x00, 0x43, 0x00, 0x02, 0x01, 0x01, 0x02, 0x01, 0x01, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x03, 0x05, 0x03, 0x03, 0x03, 0x03, 0x03, 0x06, 0x04,
            0x04, 0x03, 0x05, 0x07, 0x06, 0x07, 0x07, 0x07, 0x06, 0x07, 0x07, 0x08, 0x09, 0x0b, 0x09, 0x08,
            0x08, 0x0a, 0x08, 0x07, 0x07, 0x0a, 0x0d, 0x0a, 0x0a, 0x0b, 0x0c, 0x0c, 0x0c, 0x0c, 0x07, 0x09,
            0x0e, 0x0f, 0x0d, 0x0c, 0x0e, 0x0b, 0x0c, 0x0c, 0x0c, 0xff, 0xdb, 0x00, 0x43, 0x01, 0x02, 0x02,
            0x02, 0x03, 0x03, 0x03, 0x06, 0x03, 0x03, 0x06, 0x0c, 0x08, 0x07, 0x08, 0x0c, 0x0c, 0x0c, 0x0c,
            0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c,
            0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c,
            0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0xff, 0xc0,
            0x00, 0x11, 0x08, 0x00, 0x48, 0x00, 0x80, 0x03, 0x01, 0x22, 0x00, 0x02, 0x11, 0x01, 0x03, 0x11,
            0x01, 0xff, 0xc4, 0x00, 0x1f, 0x00, 0x00, 0x01, 0x05, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
            0x0a, 0x0b, 0xff, 0xc4, 0x00, 0xb5, 0x10, 0x00, 0x02, 0x01, 0x03, 0x03, 0x02, 0x04, 0x03, 0x05,
            0x05, 0x04, 0x04, 0x00, 0x00, 0x01, 0x7d, 0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21,
            0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07, 0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08, 0x23,
            0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0, 0x24, 0x33, 0x62, 0x72, 0x82, 0x09, 0x0a, 0x16, 0x17,
            0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a,
            0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a,
            0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a,
            0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99,
            0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7,
            0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5,
            0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf1,
            0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9, 0xfa, 0xff, 0xc4, 0x00, 0x1f, 0x01, 0x00, 0x03,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
            0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0xff, 0xc4, 0x00, 0xb5, 0x11, 0x00,
            0x02, 0x01, 0x02, 0x04, 0x04, 0x03, 0x04, 0x07, 0x05, 0x04, 0x04, 0x00, 0x01, 0x02, 0x77, 0x00,
            0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21, 0x31, 0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71, 0x13,
            0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91, 0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0, 0x15,
            0x62, 0x72, 0xd1, 0x0a, 0x16, 0x24, 0x34, 0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26, 0x27,
            0x28, 0x29, 0x2a, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
            0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
            0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88,
            0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6,
            0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4,
            0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe2,
            0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8, 0xf9,
            0xfa, 0xff, 0xda, 0x00, 0x0c, 0x03, 0x01, 0x00, 0x02, 0x11, 0x03, 0x11, 0x00, 0x3f, 0x00, 0xfa,
            0x22, 0x8a, 0x28, 0xaf, 0xed, 0x03, 0xfb, 0x30, 0x28, 0xa2, 0x8a, 0x00, 0x28, 0xa2, 0x8a, 0x00,
            0x28, 0xa2, 0x8a, 0x00, 0x28, 0xa2, 0x8a, 0x00, 0x28, 0xa2, 0x8a, 0x00, 0x28, 0xa2, 0x8a, 0x00,
            0x28, 0xa2, 0x8a, 0x00, 0x2b, 0xe6, 0x8f, 0xf8, 0x28, 0xd7, 0xfc, 0x14, 0x55, 0x7f, 0x60, 0x1d,
            0x37, 0xc2, 0x72, 0x2f, 0x83, 0xa6, 0xf1, 0x85, 0xd7, 0x8a, 0xe7, 0xb8, 0x86, 0x38, 0x57, 0x52,
            0xfb, 0x08, 0x84, 0x44, 0x23, 0xc9, 0xdd, 0xe5, 0x4a, 0x58, 0x93, 0x22, 0x80, 0x36, 0x8e, 0xfc,
            0xf6, 0xaf, 0xa5, 0xeb, 0xf3, 0x57, 0xfe, 0x0e, 0x16, 0xff, 0x00, 0x8f, 0xef, 0x82, 0x9f, 0xf6,
            0x11, 0xbe, 0xfe, 0x76, 0x95, 0xf3, 0x1c, 0x5d, 0x8e, 0xc4, 0xe1, 0x30, 0x0a, 0x78, 0x49, 0xf2,
            0x4d, 0xce, 0x11, 0xbd, 0x93, 0xb2, 0x94, 0xd2, 0x7a, 0x34, 0xd6, 0xcc, 0xf9, 0xfe, 0x2a, 0xc7,
            0x56, 0xc1, 0x65, 0x18, 0x8c, 0x5e, 0x1d, 0xda, 0x70, 0x8d, 0xd3, 0xdf, 0x5f, 0x9e, 0x87, 0x56,
            0xbf, 0xf0, 0x58, 0xcf, 0x8b, 0x4c, 0xb9, 0x1f, 0xb2, 0x3f, 0xc4, 0x52, 0x0f, 0x20, 0x8b, 0xab,
            0xce, 0x7f, 0xf2, 0x9d, 0x5e, 0xf1, 0xfb, 0x0f, 0x7e, 0xda, 0x1e, 0x31, 0xfd, 0xab, 0xf5, 0x4f,
            0x10, 0xc1, 0xe2, 0x8f, 0x83, 0x7e, 0x27, 0xf8, 0x5d, 0x16, 0x8d, 0x14, 0x32, 0x5b, 0xcf, 0xaa,
            0x4b, 0x33, 0xad, 0xfb, 0x39, 0x70, 0x55, 0x7c, 0xcb, 0x68, 0x79, 0x5d, 0xa0, 0x9c, 0x6e, 0xfb,
            0xdd, 0xbb, 0xfd, 0x05, 0x63, 0xff, 0x00, 0x1e, 0x50, 0xff, 0x00, 0xb8, 0xbf, 0xca, 0xa5, 0xae,
            0xbc, 0x0e, 0x5d, 0x8f, 0xa1, 0x5f, 0x9f, 0x11, 0x8b, 0x75, 0x22, 0xaf, 0xee, 0xb8, 0x42, 0x37,
            0xd3, 0x4d, 0x62, 0x93, 0xd3, 0x7f, 0xf8, 0x04, 0x60, 0xb2, 0xec, 0xca, 0x33, 0x85, 0x5a, 0xd8,
            0xc7, 0x38, 0xee, 0xe3, 0xc9, 0x05, 0x7f, 0x2b, 0xa5, 0x70, 0xa2, 0xbe, 0x5a, 0xf8, 0xf5, 0xff,
            0x00, 0x05, 0x45, 0xd0, 0x3f, 0x67, 0x2f, 0xdb, 0x1e, 0xd7, 0xe1, 0x87, 0x89, 0x74, 0x54, 0xb2,
            0xd1, 0x1b, 0x46, 0x6d, 0x62, 0xef, 0xc4, 0xb2, 0x6a, 0x38, 0x5b, 0x40, 0x21, 0x9a, 0x5d, 0x82,
            0xd8, 0x44, 0x4b, 0x93, 0xe5, 0x05, 0x18, 0x93, 0x24, 0xb8, 0x01, 0x49, 0xeb, 0xc2, 0xfc, 0x0a,
            0xff, 0x00, 0x82, 0xcf, 0x45, 0xf1, 0xe3, 0xe3, 0xdf, 0x87, 0x3c, 0x37, 0x65, 0xf0, 0xa3, 0xc4,
            0xfa, 0x77, 0x84, 0xbc, 0x59, 0xa8, 0xb6, 0x9b, 0xa6, 0x78, 0x96, 0xee, 0xeb, 0x6a, 0xcb, 0x20,
            0x07, 0x19, 0x88, 0x44, 0x63, 0xea, 0x39, 0x0b, 0x3b, 0x11, 0xef, 0xd2, 0xa2, 0x3c, 0x4f, 0x95,
            0xcb, 0x11, 0x1c, 0x24, 0x6a, 0xde, 0xa4, 0xa4, 0xe0, 0x95, 0xa5, 0xf1, 0x29, 0x72, 0x59, 0xe9,
            0xa7, 0xbd, 0xa2, 0x6e, 0xc9, 0xbd, 0x9b, 0x35, 0xaf, 0xc4, 0xf9, 0x65, 0x0a, 0xae, 0x8d, 0x5a,
            0xb6, 0x92, 0x97, 0x2b, 0x56, 0x77, 0xbd, 0x93, 0xed, 0xb5, 0x9a, 0xd7, 0x6f, 0x33, 0xed, 0xfa,
            0x2b, 0xe5, 0xaf, 0x8f, 0x5f, 0xf0, 0x54, 0x5d, 0x03, 0xf6, 0x72, 0xfd, 0xb1, 0xed, 0x7e, 0x18,
            0x78, 0x97, 0x45, 0x4b, 0x2d, 0x11, 0xb4, 0x66, 0xd6, 0x2e, 0xfc, 0x4b, 0x26, 0xa3, 0x85, 0xb4,
            0x02, 0x19, 0xa5, 0xd8, 0x2d, 0x84, 0x44, 0xb9, 0x3e, 0x50, 0x51, 0x89, 0x32, 0x4b, 0x80, 0x14,
            0x9e, 0xbc, 0x2f, 0xc0, 0xaf, 0xf8, 0x2c, 0xf4, 0x5f, 0x1e, 0x3e, 0x3d, 0xf8, 0x73, 0xc3, 0x76,
            0x5f, 0x0a, 0x3c, 0x4f, 0xa7, 0x78, 0x4b, 0xc5, 0x9a, 0x8b, 0x69, 0xba, 0x67, 0x89, 0x6e, 0xee,
            0xb6, 0xac, 0xb2, 0x00, 0x71, 0x98, 0x84, 0x46, 0x3e, 0xa3, 0x90, 0xb3, 0xb1, 0x1e, 0xfd, 0x28,
            0x8f, 0x13, 0xe5, 0x72, 0xc4, 0x47, 0x09, 0x1a, 0xb7, 0xa9, 0x29, 0x38, 0x25, 0x69, 0x7c, 0x4a,
            0x5c, 0x96, 0x7a, 0x69, 0xef, 0x68, 0x9b, 0xb2, 0x6f, 0x66, 0xc2, 0xbf, 0x13, 0xe5, 0x94, 0x2a,
            0xba, 0x35, 0x6a, 0xda, 0x4a, 0x5c, 0xad, 0x59, 0xde, 0xf6, 0x4f, 0xb6, 0xd6, 0x6b, 0x5d, 0xbc,
            0xcf, 0xb7, 0xe8, 0xaf, 0x15, 0xfd, 0xb2, 0xbf, 0x6f, 0x7f, 0x87, 0xff, 0x00, 0xb0, 0xf7, 0x86,
            0x2d, 0xae, 0xfc, 0x5d, 0x77, 0x73, 0x71, 0xa9, 0xea, 0x20, 0x9b, 0x0d, 0x1f, 0x4f, 0x45, 0x96,
            0xf6, 0xf0, 0x0e, 0x0b, 0x85, 0x66, 0x55, 0x48, 0xc1, 0xea, 0xec, 0x40, 0xec, 0x32, 0x78, 0xaf,
            0x97, 0xae, 0xbf, 0xe0, 0xba, 0x1e, 0x23, 0xd2, 0x34, 0xc8, 0xf5, 0xdd, 0x47, 0xf6, 0x73, 0xf1,
            0xc5, 0x8f, 0x83, 0x24, 0x2a, 0xc3, 0x5b, 0x7b, 0xd9, 0x44, 0x46, 0x36, 0xe8, 0xe3, 0x75, 0xa2,
            0xc4, 0x73, 0xd8, 0x79, 0xb8, 0x3e, 0xb5, 0x38, 0xce, 0x2a, 0xca, 0xb0, 0xb5, 0x9d, 0x0a, 0xf5,
            0xad, 0x28, 0xef, 0x65, 0x26, 0xa3, 0xfe, 0x26, 0x93, 0x51, 0xf9, 0xb4, 0x56, 0x61, 0xc4, 0x79,
            0x76, 0x06, 0xaf, 0xb1, 0xc4, 0xd5, 0xb4, 0xad, 0x7b, 0x24, 0xe4, 0xd2, 0xee, 0xf9, 0x53, 0xb2,
            0xf5, 0xb1, 0xfa, 0x17, 0x45, 0x79, 0x67, 0xec, 0x9b, 0xfb, 0x62, 0xf8, 0x1f, 0xf6, 0xcf, 0xf8,
            0x7a, 0xfe, 0x21, 0xf0, 0x55, 0xf4, 0xd2, 0xa5, 0xac, 0x82, 0x1b, 0xeb, 0x1b, 0xa8, 0xc4, 0x57,
            0x9a, 0x74, 0x84, 0x64, 0x2c, 0xa9, 0x92, 0x39, 0x19, 0xc3, 0x29, 0x2a, 0xd8, 0x38, 0x27, 0x07,
            0x1c, 0x17, 0xed, 0x91, 0xff, 0x00, 0x05, 0x11, 0xb1, 0xfd, 0x8f, 0x7e, 0x39, 0x7c, 0x37, 0xf0,
            0x8e, 0xa3, 0xe1, 0xe4, 0xbf, 0xb2, 0xf1, 0xe4, 0xe5, 0x2e, 0x75, 0x59, 0x35, 0x41, 0x69, 0x1e,
            0x8f, 0x18, 0x9a, 0x38, 0xcc, 0x8c, 0x86, 0x36, 0x0e, 0x00, 0x72, 0xc7, 0x2c, 0x98, 0x0b, 0xd7,
            0x9e, 0x3d, 0x1c, 0x46, 0x6b, 0x84, 0xa1, 0x4a, 0x15, 0xea, 0xd4, 0x4a, 0x13, 0x69, 0x45, 0xee,
            0x9b, 0x96, 0xda, 0xad, 0x2c, 0xfb, 0xed, 0xdd, 0x9d, 0x13, 0xce, 0x70, 0x51, 0xc1, 0x7f, 0x68,
            0xfb, 0x44, 0xe8, 0xe9, 0xef, 0x2b, 0xb5, 0xab, 0xe5, 0xe9, 0x77, 0xf1, 0x3b, 0x3d, 0x34, 0x7b,
            0x9f, 0x48, 0x57, 0x96, 0xfe, 0xdb, 0x1f, 0x12, 0xb5, 0x8f, 0x83, 0xbf, 0xb2, 0x67, 0xc4, 0x0f,
            0x14, 0x78, 0x7e, 0xe5, 0x6c, 0xb5, 0xbd, 0x0f, 0x46, 0x9e, 0xee, 0xca, 0x76, 0x89, 0x25, 0x10,
            0xca, 0xa3, 0xe5, 0x6d, 0xae, 0x0a, 0xb6, 0x3d, 0x08, 0x22, 0xbe, 0x5e, 0xba, 0xff, 0x00, 0x82,
            0xe9, 0x69, 0x1a, 0xd7, 0xc5, 0x18, 0xac, 0x7c, 0x2b, 0xf0, 0xc3, 0xc5, 0x1e, 0x22, 0xf0, 0x3f,
            0xf6, 0xac, 0x5a, 0x54, 0xde, 0x2a, 0x33, 0x9b, 0x68, 0x91, 0xe4, 0x70, 0x81, 0xd6, 0x2f, 0x25,
            0x97, 0x07, 0x20, 0xa8, 0x79, 0x51, 0x88, 0x23, 0x2a, 0xa7, 0x8a, 0xfa, 0x2b, 0xfe, 0x0a, 0x33,
            0x69, 0xf6, 0xdf, 0xd8, 0x47, 0xe2, 0xc2, 0x6e, 0xdb, 0x8f, 0x0c, 0xde, 0x49, 0x9c, 0x67, 0xee,
            0xc6, 0x5b, 0x1f, 0xa5, 0x78, 0x39, 0x96, 0x79, 0x43, 0x19, 0x92, 0x63, 0x31, 0x39, 0x75, 0x46,
            0xdc, 0x29, 0xd4, 0xb3, 0x57, 0x8b, 0x4d, 0x41, 0xb4, 0xd3, 0x69, 0x3e, 0xcd, 0x35, 0xa7, 0x66,
            0x63, 0x43, 0x3a, 0xc1, 0xe3, 0x61, 0x5a, 0x38, 0x3a, 0x9c, 0xce, 0x0b, 0x5b, 0x5e, 0xda, 0xde,
            0xd6, 0x7b, 0x3d, 0x9e, 0xa9, 0xbd, 0x8e, 0x37, 0xfe, 0x09, 0x25, 0xfb, 0x43, 0x78, 0xcf, 0xf6,
            0x9d, 0xfd, 0x91, 0x20, 0xf1, 0x47, 0x8e, 0xb5, 0x48, 0xb5, 0x9d, 0x6d, 0xb5, 0x7b, 0xbb, 0x55,
            0xba, 0x4b, 0x58, 0xad, 0x8b, 0xc2, 0x9b, 0x36, 0x86, 0x48, 0x95, 0x53, 0x20, 0x96, 0xe8, 0xbd,
            0x31, 0x9e, 0x6b, 0xe9, 0xba, 0xf8, 0xdb, 0xfe, 0x08, 0x43, 0x6e, 0xf0, 0x7f, 0xc1, 0x3f, 0xf4,
            0xd6, 0x61, 0x81, 0x36, 0xb7, 0x7e, 0xe8, 0x73, 0xd4, 0x6f, 0x55, 0xfe, 0x60, 0xd7, 0xd9, 0x35,
            0xef, 0x65, 0x13, 0x94, 0xf2, 0xec, 0x35, 0x49, 0xbb, 0xb9, 0x52, 0xa6, 0xdb, 0x7b, 0xb6, 0xe1,
            0x16, 0xdb, 0xf3, 0x6f, 0x53, 0x93, 0x83, 0xeb, 0x55, 0xad, 0x93, 0x50, 0xab, 0x5a, 0x4e, 0x52,
            0x6b, 0x56, 0xdd, 0xdb, 0xd5, 0xf5, 0x61, 0x5f, 0x99, 0xff, 0x00, 0xf0, 0x71, 0x35, 0xb3, 0x5e,
            0x0f, 0x83, 0x71, 0x24, 0x8d, 0x0b, 0xcb, 0x7b, 0xa8, 0x22, 0xc8, 0xbd, 0x63, 0x27, 0xec, 0xa0,
            0x30, 0xfa, 0x57, 0xe9, 0x85, 0x7e, 0x69, 0xff, 0x00, 0xc1, 0xc3, 0x93, 0x2c, 0x17, 0x3f, 0x05,
            0x9d, 0xd9, 0x51, 0x13, 0x50, 0xbf, 0x66, 0x66, 0x38, 0x0a, 0x01, 0xb4, 0xc9, 0x26, 0xbe, 0x6b,
            0x8f, 0xa3, 0x19, 0x65, 0x6a, 0x33, 0xd9, 0xd4, 0xa5, 0x7e, 0x9a, 0x73, 0xae, 0xbd, 0x0c, 0xb8,
            0xe1, 0xb5, 0x90, 0x62, 0xda, 0xdf, 0x91, 0xfe, 0x68, 0xea, 0xed, 0xbf, 0xe0, 0x8e, 0xff, 0x00,
            0x16, 0xa4, 0xb7, 0x46, 0x1f, 0xb5, 0xbf, 0xc4, 0x55, 0x05, 0x41, 0x00, 0x5a, 0xde, 0x71, 0xff,
            0x00, 0x95, 0x1a, 0xfa, 0xbf, 0xf6, 0x40, 0xf8, 0x03, 0xaf, 0xfe, 0xcd, 0x7f, 0x07, 0x53, 0xc3,
            0x3e, 0x23, 0xf1, 0xf6, 0xb1, 0xf1, 0x1f, 0x50, 0x4b, 0xc9, 0xae, 0x46, 0xaf, 0xa9, 0xa3, 0xac,
            0xc1, 0x1f, 0x18, 0x88, 0x07, 0x96, 0x56, 0xda, 0xb8, 0x27, 0x97, 0x3f, 0x78, 0xf4, 0xe9, 0x54,
            0x2c, 0xff, 0x00, 0x6f, 0xaf, 0x82, 0x0b, 0x69, 0x10, 0x3f, 0x16, 0xfe, 0x1d, 0x02, 0x10, 0x02,
            0x0e, 0xbf, 0x6d, 0xc7, 0x1f, 0xef, 0xd7, 0x5f, 0xf0, 0xb7, 0xf6, 0x88, 0xf0, 0x17, 0xc6, 0xfb,
            0x9b, 0xb8, 0x7c, 0x1d, 0xe3, 0x2f, 0x0c, 0xf8, 0xa2, 0x6b, 0x05, 0x57, 0xb9, 0x8f, 0x4b, 0xd4,
            0xa2, 0xba, 0x6b, 0x75, 0x62, 0x42, 0x96, 0x08, 0xc4, 0x80, 0x48, 0x38, 0x27, 0xd2, 0xbd, 0x3c,
            0xa7, 0x2f, 0xca, 0x70, 0xb5, 0xdc, 0xb0, 0x12, 0x5c, 0xf2, 0x4d, 0x7f, 0x12, 0x53, 0xd3, 0x47,
            0xb3, 0x93, 0xed, 0xba, 0xd7, 0xe5, 0x72, 0x72, 0x9c, 0xb7, 0x26, 0xc3, 0xd5, 0x85, 0x5c, 0x2c,
            0x97, 0x3d, 0xbf, 0xe7, 0xe4, 0xa5, 0xba, 0xec, 0xe4, 0xd7, 0xe0, 0x7e, 0x6c, 0x7f, 0xc1, 0x40,
            0xfe, 0x0f, 0x69, 0x1f, 0x1f, 0x7f, 0xe0, 0xb6, 0x1e, 0x04, 0xf0, 0x86, 0xbe, 0xb3, 0x49, 0xa3,
            0x6b, 0x56, 0x16, 0x11, 0xde, 0x47, 0x13, 0xec, 0x69, 0x63, 0x51, 0x71, 0x21, 0x4d, 0xc3, 0x90,
            0x1b, 0x66, 0xd2, 0x47, 0x38, 0x27, 0x18, 0x35, 0xfa, 0x9f, 0xa1, 0xe8, 0x96, 0x7e, 0x19, 0xd1,
            0xad, 0x34, 0xed, 0x3a, 0xd6, 0x0b, 0x2b, 0x0b, 0x08, 0x52, 0xde, 0xda, 0xde, 0x08, 0xc2, 0x45,
            0x04, 0x68, 0x02, 0xaa, 0x2a, 0x8e, 0x02, 0x80, 0x00, 0x00, 0x7a, 0x57, 0xe6, 0xd7, 0xed, 0x2f,
            0xff, 0x00, 0x29, 0xff, 0x00, 0xf8, 0x61, 0xff, 0x00, 0x5e, 0x36, 0x9f, 0xfa, 0x26, 0xea, 0xbf,
            0x4b, 0xeb, 0xcc, 0xe0, 0x6a, 0x50, 0x54, 0x71, 0xd5, 0x12, 0xf7, 0x9e, 0x2a, 0xba, 0x6f, 0xba,
            0x4e, 0x36, 0x5f, 0x2b, 0xbb, 0x7a, 0xb3, 0x9f, 0x87, 0xa8, 0xc3, 0xfb, 0x5b, 0x30, 0xab, 0x6f,
            0x7b, 0x9a, 0x2a, 0xfd, 0x6d, 0xca, 0xb4, 0x3f, 0x29, 0xbf, 0xe0, 0xa0, 0x7f, 0x07, 0xb4, 0x8f,
            0x8f, 0xbf, 0xf0, 0x5b, 0x0f, 0x02, 0x78, 0x43, 0x5f, 0x59, 0xa4, 0xd1, 0xb5, 0xab, 0x0b, 0x08,
            0xef, 0x23, 0x89, 0xf6, 0x34, 0xb1, 0xa8, 0xb8, 0x90, 0xa6, 0xe1, 0xc8, 0x0d, 0xb3, 0x69, 0x23,
            0x9c, 0x13, 0x8c, 0x1a, 0xfd, 0x4f, 0xd0, 0xf4, 0x4b, 0x3f, 0x0c, 0xe8, 0xd6, 0x9a, 0x76, 0x9d,
            0x6b, 0x05, 0x95, 0x85, 0x84, 0x29, 0x6f, 0x6d, 0x6f, 0x04, 0x61, 0x22, 0x82, 0x34, 0x01, 0x55,
            0x15, 0x47, 0x01, 0x40, 0x00, 0x00, 0x3d, 0x2b, 0xf3, 0x6b, 0xf6, 0x97, 0xff, 0x00, 0x94, 0xff,
            0x00, 0xfc, 0x30, 0xff, 0x00, 0xaf, 0x1b, 0x4f, 0xfd, 0x13, 0x75, 0x5f, 0xa5, 0xf4, 0x70, 0x35,
            0x28, 0x2a, 0x38, 0xea, 0x89, 0x7b, 0xcf, 0x15, 0x5d, 0x37, 0xdd, 0x27, 0x1b, 0x2f, 0x95, 0xdd,
            0xbd, 0x58, 0x70, 0xf5, 0x18, 0x7f, 0x6b, 0x66, 0x15, 0x6d, 0xef, 0x73, 0x45, 0x5f, 0xad, 0xb9,
            0x56, 0x87, 0xe5, 0xc7, 0xc3, 0x1d, 0x12, 0xd3, 0xf6, 0xc7, 0xff, 0x00, 0x82, 0xeb, 0x78, 0xba,
            0x7f, 0x12, 0xc1, 0x1e, 0xab, 0xa4, 0xfc, 0x3d, 0x59, 0xfe, 0xc5, 0x69, 0x70, 0x37, 0x44, 0x86,
            0xc8, 0xc7, 0x04, 0x43, 0x69, 0xe0, 0x81, 0x3c, 0x8d, 0x26, 0x3a, 0x6e, 0xe6, 0xbf, 0x50, 0xee,
            0xad, 0x62, 0xbe, 0xb5, 0x92, 0x09, 0xe3, 0x49, 0xa1, 0x99, 0x4a, 0x49, 0x1b, 0xa8, 0x65, 0x75,
            0x23, 0x04, 0x10, 0x78, 0x20, 0x8e, 0xd5, 0xf9, 0x5d, 0xf1, 0xb7, 0xc4, 0x93, 0xff, 0x00, 0xc1,
            0x32, 0xbf, 0xe0, 0xb0, 0x17, 0x5f, 0x11, 0x35, 0xeb, 0x3b, 0xe9, 0x7c, 0x03, 0xf1, 0x0d, 0x25,
            0x96, 0x4b, 0xc8, 0xa0, 0x32, 0x15, 0x8e, 0x75, 0x5f, 0x38, 0x29, 0xee, 0xf1, 0x4e, 0x8a, 0xe5,
            0x41, 0xdd, 0xe5, 0xb2, 0xf5, 0xdc, 0x33, 0xf6, 0xbf, 0x89, 0x3f, 0xe0, 0xa8, 0xff, 0x00, 0x00,
            0xfc, 0x35, 0xe0, 0x16, 0xf1, 0x0b, 0x7c, 0x4d, 0xf0, 0xdd, 0xed, 0xb7, 0x92, 0x25, 0x4b, 0x4b,
            0x39, 0xbc, 0xeb, 0xf9, 0x73, 0xd1, 0x45, 0xb0, 0x1e, 0x6a, 0xb1, 0xe9, 0x86, 0x55, 0xc7, 0xf1,
            0x60, 0x73, 0x5c, 0x1c, 0x1f, 0x98, 0x60, 0x70, 0xb9, 0x5c, 0xe8, 0xe3, 0x6a, 0x46, 0x15, 0x63,
            0x3a, 0x9e, 0xd5, 0x49, 0xa4, 0xdc, 0x9c, 0x9e, 0xad, 0x3d, 0xd3, 0x56, 0xb3, 0xd5, 0x3e, 0x87,
            0x3f, 0x0e, 0xe3, 0x30, 0xf8, 0x5c, 0x6e, 0x61, 0x4f, 0x17, 0x35, 0x0a, 0xae, 0xb4, 0xa5, 0xef,
            0x34, 0xaf, 0x0b, 0x2e, 0x46, 0xaf, 0xf6, 0x52, 0xdb, 0xb7, 0x95, 0xcf, 0x8d, 0x3f, 0x61, 0xbd,
            0x1a, 0xdf, 0xf6, 0x60, 0xff, 0x00, 0x82, 0xd9, 0xfc, 0x41, 0xf8, 0x77, 0xe1, 0xe5, 0x6b, 0x4f,
            0x0d, 0x6a, 0xd0, 0x5d, 0x47, 0x1d, 0x94, 0x6d, 0xfb, 0xa8, 0x57, 0xc9, 0x8e, 0xf6, 0x25, 0xc1,
            0xed, 0x18, 0x2c, 0x8b, 0xe8, 0x1b, 0xeb, 0x53, 0x7f, 0xc1, 0x79, 0x7c, 0x29, 0x0f, 0x8f, 0x3f,
            0x69, 0xff, 0x00, 0x81, 0x9a, 0x1d, 0xcb, 0xc9, 0x15, 0xbe, 0xb3, 0xe6, 0x58, 0xca, 0xf1, 0xe3,
            0x7a, 0x24, 0xb7, 0x70, 0x21, 0x23, 0x3c, 0x64, 0x06, 0x38, 0xad, 0x5f, 0xf8, 0x25, 0x37, 0x82,
            0xb5, 0xef, 0xda, 0xbb, 0xf6, 0xe6, 0xf8, 0x83, 0xfb, 0x49, 0x6a, 0x9a, 0x5c, 0xba, 0x5f, 0x87,
            0xae, 0x9e, 0xe6, 0xd7, 0x47, 0xf3, 0x94, 0x83, 0x3c, 0xb2, 0x6d, 0x88, 0x04, 0x3c, 0x86, 0x11,
            0x40, 0x9b, 0x1d, 0x81, 0xc6, 0xe7, 0x00, 0x74, 0x20, 0x27, 0xfc, 0x16, 0xb7, 0xfe, 0x4f, 0x53,
            0xf6, 0x74, 0xff, 0x00, 0xaf, 0xe4, 0xff, 0x00, 0xd2, 0xfb, 0x6a, 0xf9, 0xe8, 0x61, 0x6f, 0xc3,
            0x38, 0x0a, 0x55, 0x63, 0xee, 0x4a, 0xb4, 0x6d, 0x17, 0xfc, 0x92, 0x9c, 0x9a, 0x56, 0xf3, 0x4e,
            0xfe, 0x8c, 0xf9, 0x6c, 0x4a, 0xbf, 0x0b, 0xe6, 0xb5, 0xa9, 0x2b, 0x52, 0x9d, 0x67, 0x2a, 0x7d,
            0x17, 0x27, 0xb4, 0xa6, 0x95, 0xbc, 0xae, 0x99, 0xfa, 0x1f, 0xf0, 0xaf, 0xe1, 0x76, 0x85, 0xf0,
            0x57, 0xe1, 0xe6, 0x93, 0xe1, 0x6f, 0x0d, 0x69, 0xf0, 0xe9, 0x9a, 0x1e, 0x89, 0x6e, 0xb6, 0xd6,
            0xb6, 0xf1, 0x8f, 0xba, 0xa3, 0xa9, 0x27, 0xab, 0x33, 0x1c, 0xb3, 0x31, 0xe5, 0x89, 0x24, 0xf2,
            0x6b, 0x80, 0xff, 0x00, 0x82, 0x80, 0xda, 0xbd, 0xef, 0xec, 0x3f, 0xf1, 0x62, 0x38, 0xd7, 0x73,
            0xb7, 0x85, 0x75, 0x0c, 0x0c, 0xe3, 0x3f, 0xb8, 0x7a, 0xf5, 0xfa, 0xf3, 0xdf, 0xda, 0xbb, 0xc6,
            0xfe, 0x13, 0xf8, 0x77, 0xfb, 0x38, 0xf8, 0xc7, 0x56, 0xf1, 0xd5, 0xad, 0xd5, 0xf7, 0x84, 0x60,
            0xd3, 0x25, 0x8f, 0x55, 0xb5, 0xb6, 0x56, 0x33, 0x5d, 0x43, 0x20, 0xf2, 0xcc, 0x49, 0x86, 0x53,
            0xb9, 0xb7, 0x85, 0x07, 0x72, 0xe3, 0x39, 0xdc, 0x3a, 0xd7, 0xe9, 0x9c, 0x45, 0x08, 0xcf, 0x2a,
            0xc4, 0xd3, 0x9c, 0x94, 0x53, 0xa7, 0x35, 0x77, 0xa2, 0x57, 0x8b, 0x57, 0x76, 0xe8, 0xbc, 0x91,
            0xfa, 0xe7, 0xb1, 0xa5, 0x4a, 0x87, 0xb1, 0x8a, 0x51, 0x82, 0x56, 0xec, 0x92, 0x4b, 0xf2, 0x48,
            0xf0, 0x3f, 0xf8, 0x21, 0xa4, 0x4d, 0x1f, 0xfc, 0x13, 0xcf, 0xc3, 0xdb, 0x95, 0x97, 0x76, 0xa7,
            0xa8, 0x32, 0xe4, 0x7d, 0xe1, 0xf6, 0x86, 0xe4, 0x57, 0xd7, 0xd5, 0xe0, 0x1f, 0xf0, 0x4d, 0x6f,
            0x8a, 0x1f, 0x0b, 0x3e, 0x26, 0xfe, 0xcc, 0xd6, 0xc7, 0xe0, 0xfe, 0x8d, 0xaa, 0xf8, 0x77, 0xc2,
            0x3a, 0x45, 0xfd, 0xc5, 0x90, 0xd3, 0x75, 0x26, 0x66, 0xb9, 0xb5, 0x98, 0x91, 0x2b, 0xee, 0x66,
            0x96, 0x62, 0x77, 0x79, 0x81, 0x81, 0xf3, 0x0f, 0x0d, 0x8e, 0x31, 0x81, 0xef, 0xf5, 0xd7, 0x95,
            0xc6, 0x30, 0xc0, 0x61, 0xe9, 0xc1, 0xf3, 0x28, 0xd3, 0xa6, 0x93, 0x5b, 0x3b, 0x42, 0x2a, 0xeb,
            0xc9, 0xda, 0xe8, 0xf1, 0xf8, 0x52, 0x94, 0x69, 0x65, 0x14, 0x29, 0xc2, 0x6a, 0x69, 0x47, 0x78,
            0xec, 0xf5, 0x77, 0xb5, 0xed, 0xd7, 0xc8, 0x2b, 0xcd, 0x3f, 0x68, 0xff, 0x00, 0xd8, 0xff, 0x00,
            0xe1, 0xcf, 0xed, 0x6f, 0xa7, 0x69, 0x76, 0xbf, 0x10, 0xbc, 0x37, 0x1f, 0x88, 0x21, 0xd1, 0x64,
            0x79, 0x6c, 0xb3, 0x79, 0x71, 0x6a, 0xd0, 0x33, 0x80, 0x1f, 0x0d, 0x04, 0x88, 0xc4, 0x10, 0xab,
            0x90, 0x49, 0x1c, 0x0e, 0x38, 0xaf, 0x4b, 0xa2, 0xb6, 0xc4, 0xe1, 0x68, 0xe2, 0x69, 0xba, 0x58,
            0x88, 0x29, 0xc5, 0xf4, 0x92, 0x4d, 0x69, 0xe4, 0xcf, 0x72, 0xb5, 0x1a, 0x75, 0xa0, 0xe9, 0x55,
            0x8a, 0x94, 0x5e, 0xe9, 0xab, 0xa7, 0xea, 0x99, 0xf2, 0xef, 0xfc, 0x39, 0x83, 0xf6, 0x6a, 0xff,
            0x00, 0xa2, 0x6d, 0xff, 0x00, 0x97, 0x06, 0xab, 0xff, 0x00, 0xc9, 0x35, 0xe9, 0x1f, 0xb3, 0x8f,
            0xec, 0x31, 0xf0, 0xaf, 0xf6, 0x49, 0xd6, 0x35, 0x2b, 0xff, 0x00, 0x87, 0xde, 0x16, 0x1a, 0x05,
            0xe6, 0xaf, 0x0a, 0x5b, 0xdd, 0xcb, 0xfd, 0xa3, 0x77, 0x76, 0x65, 0x8d, 0x58, 0xb2, 0xaf, 0xef,
            0xe5, 0x70, 0xbc, 0x9c, 0xfc, 0xb8, 0xcf, 0x19, 0xe9, 0x5e, 0xb5, 0x45, 0x72, 0x61, 0xb2, 0x6c,
            0xbf, 0x0d, 0x51, 0x56, 0xc3, 0xd0, 0x84, 0x24, 0xaf, 0x67, 0x18, 0xc5, 0x3d, 0x55, 0x9e, 0xa9,
            0x5f, 0x54, 0xec, 0xfc, 0x8f, 0x3e, 0x8e, 0x45, 0x96, 0xd2, 0x9a, 0xa9, 0x4b, 0x0f, 0x08, 0xc9,
            0x6c, 0xd4, 0x22, 0x9a, 0xf4, 0x76, 0x3c, 0xd7, 0xc4, 0xbf, 0xb2, 0x17, 0xc3, 0xaf, 0x17, 0xfc,
            0x7f, 0xd2, 0xfe, 0x28, 0xea, 0x3e, 0x1d, 0x5b, 0x8f, 0x1d, 0xe8, 0xb1, 0x2c, 0x36, 0x7a, 0x9f,
            0xdb, 0x6e, 0x57, 0xc9, 0x55, 0x0e, 0x14, 0x79, 0x4b, 0x20, 0x89, 0xb0, 0x24, 0x7e, 0x59, 0x09,
            0xe7, 0xd8, 0x57, 0xa5, 0x51, 0x45, 0x76, 0xd1, 0xa1, 0x4a, 0x8a, 0x71, 0xa3, 0x15, 0x14, 0xdb,
            0x93, 0xb2, 0xb5, 0xe4, 0xf7, 0x6e, 0xdb, 0xb7, 0xd5, 0xee, 0xce, 0xfa, 0x78, 0x7a, 0x54, 0xe5,
            0x29, 0xd3, 0x8a, 0x4e, 0x5a, 0xb6, 0x95, 0xae, 0xfb, 0xbe, 0xff, 0x00, 0x33, 0xcd, 0x7c, 0x4b,
            0xfb, 0x21, 0x7c, 0x3a, 0xf1, 0x7f, 0xc7, 0xfd, 0x2f, 0xe2, 0x8e, 0xa3, 0xe1, 0xd5, 0xb8, 0xf1,
            0xde, 0x8b, 0x12, 0xc3, 0x67, 0xa9, 0xfd, 0xb6, 0xe5, 0x7c, 0x95, 0x50, 0xe1, 0x47, 0x94, 0xb2,
            0x08, 0x9b, 0x02, 0x47, 0xe5, 0x90, 0x9e, 0x7d, 0x85, 0x7a, 0x55, 0x14, 0x51, 0x46, 0x85, 0x2a,
            0x29, 0xc6, 0x8c, 0x54, 0x53, 0x6e, 0x4e, 0xca, 0xd7, 0x93, 0xdd, 0xbb, 0x6e, 0xdf, 0x57, 0xbb,
            0x0a, 0x78, 0x7a, 0x54, 0xe5, 0x29, 0xd3, 0x8a, 0x4e, 0x5a, 0xb6, 0x95, 0xae, 0xfb, 0xbe, 0xff,
            0x00, 0x33, 0x03, 0xe2, 0x57, 0xc2, 0xbf, 0x0d, 0x7c, 0x64, 0xf0, 0xac, 0xba, 0x1f, 0x8a, 0xf4,
            0x2d, 0x2b, 0xc4, 0x5a, 0x44, 0xe4, 0x33, 0xda, 0x6a, 0x16, 0xc9, 0x3c, 0x5b, 0x87, 0x46, 0x01,
            0x81, 0xc3, 0x0c, 0x9c, 0x30, 0xc1, 0x1d, 0x8d, 0x78, 0x6e, 0x83, 0xff, 0x00, 0x04, 0x87, 0xfd,
            0x9c, 0xfc, 0x39, 0xae, 0xae, 0xa3, 0x6f, 0xf0, 0xcb, 0x4e, 0x92, 0xe1, 0x1f, 0x78, 0x4b, 0xad,
            0x46, 0xf6, 0xea, 0x0c, 0xe7, 0x3c, 0xc3, 0x2c, 0xcd, 0x19, 0x1e, 0xc5, 0x71, 0xdb, 0xa5, 0x7d,
            0x23, 0x45, 0x73, 0x62, 0x32, 0xbc, 0x15, 0x7a, 0x8a, 0xad, 0x7a, 0x31, 0x94, 0x97, 0x57, 0x14,
            0xdf, 0xde, 0xd1, 0x86, 0x2b, 0x2d, 0xc2, 0x62, 0x64, 0xa5, 0x89, 0xa5, 0x19, 0xb5, 0xb7, 0x34,
            0x53, 0xb7, 0xa5, 0xd1, 0x57, 0x43, 0xd0, 0xec, 0xbc, 0x33, 0xa3, 0xdb, 0x69, 0xfa, 0x6d, 0x9d,
            0xae, 0x9f, 0x61, 0x65, 0x1a, 0xc3, 0x6f, 0x6d, 0x6d, 0x12, 0xc5, 0x0c, 0x11, 0xa8, 0xc2, 0xa2,
            0x22, 0x80, 0x15, 0x40, 0xe8, 0x00, 0xc0, 0xaf, 0x3f, 0xf8, 0xd3, 0xfb, 0x21, 0x7c, 0x3a, 0xfd,
            0xa1, 0xfc, 0x6b, 0xe1, 0xbf, 0x11, 0x78, 0xc7, 0xc3, 0xab, 0xac, 0x6b, 0x1e, 0x11, 0x93, 0xcd,
            0xd2, 0x6e, 0x0d, 0xed, 0xcc, 0x1f, 0x65, 0x6d, 0xe9, 0x26, 0x76, 0xc5, 0x22, 0xab, 0xfc, 0xc8,
            0xa7, 0x0e, 0x18, 0x71, 0xee, 0x6b, 0xd2, 0xa8, 0xae, 0x9a, 0xd4, 0x29, 0x55, 0xb2, 0xab, 0x15,
            0x2b, 0x34, 0xd5, 0xd5, 0xec, 0xd6, 0xcd, 0x79, 0xae, 0x8c, 0xd6, 0xae, 0x16, 0x8d, 0x5a, 0x5e,
            0xc2, 0xac, 0x14, 0xa0, 0xed, 0xee, 0xb4, 0x9a, 0xd3, 0x6d, 0x36, 0xd2, 0xca, 0xc1, 0x58, 0x5f,
            0x12, 0xfe, 0x1a, 0xe8, 0x7f, 0x18, 0xbc, 0x07, 0xaa, 0x78, 0x67, 0xc4, 0xba, 0x74, 0x3a, 0xb6,
            0x85, 0xac, 0xc0, 0x6d, 0xef, 0x2d, 0x25, 0x2c, 0xab, 0x32, 0x1e, 0x7a, 0xa9, 0x0c, 0xa4, 0x10,
            0x08, 0x2a, 0x41, 0x04, 0x02, 0x08, 0x22, 0xb7, 0x68, 0xa7, 0x5a, 0x8d, 0x3a, 0xd4, 0xdd, 0x2a,
            0xb1, 0x52, 0x8c, 0x95, 0x9a, 0x6a, 0xe9, 0xa7, 0xba, 0x69, 0xee, 0x8d, 0xda, 0x4d, 0x59, 0x9c,
            0x57, 0xc0, 0x5f, 0xd9, 0xdf, 0xc1, 0xbf, 0xb3, 0x17, 0x80, 0xc7, 0x86, 0x7c, 0x0b, 0xa2, 0x45,
            0xa0, 0xe8, 0xa2, 0x77, 0xba, 0x36, 0xe9, 0x3c, 0xb3, 0x97, 0x95, 0xf1, 0xb9, 0xda, 0x49, 0x59,
            0x9d, 0x8f, 0x00, 0x72, 0xc7, 0x00, 0x00, 0x38, 0x15, 0xda, 0xd1, 0x45, 0x5c, 0x23, 0x18, 0x45,
            0x42, 0x0a, 0xc9, 0x24, 0x92, 0xec, 0x96, 0x89, 0x2f, 0x24, 0xb6, 0x32, 0xa1, 0x42, 0x95, 0x0a,
            0x6a, 0x95, 0x18, 0xa8, 0xc5, 0x6c, 0x92, 0xb2, 0x5f, 0x24, 0x14, 0x51, 0x45, 0x51, 0xb0, 0x51,
            0x45, 0x14, 0x00, 0x51, 0x45, 0x14, 0x00, 0x51, 0x45, 0x14, 0x00, 0x51, 0x45, 0x14, 0x00, 0x51,
            0x45, 0x14, 0x00, 0x51, 0x45, 0x14, 0x00, 0x51, 0x45, 0x14, 0x01, 0xff, 0xd9
        };
    }
}
