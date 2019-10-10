﻿using System;
using System.IO;
namespace ViretTool.DataLayer.DataIO.BlobIO.FixedSize
{
    // TODO: document blob size limitations (expectations for format checking)
    public class FixedSizeBlobReader : FizedSizeBlobIOBase
    {
        public BinaryReader BaseBinaryReader { get; private set; }
        public byte[] DatasetHeader { get; private set; }

        public long DataStartOffset { get; private set; }
        public int BlobCount { get; private set; }
        public int BlobLength { get; private set; }
        //public byte[] FiletypeMetadata { get; private set; }

        private readonly object _lockObject = new object();

        private const int BLOBLENGTH_LIMIT = 100_000;
        private const int BLOBCOUNT_LIMIT = 10_000_000;
        private const long FILEOFFSET_LIMIT = 1_000_000_000_000;


        public FixedSizeBlobReader(string filePath)
        {
            BaseBinaryReader = new BinaryReader(
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            //DatasetHeader = FileHeaderUtilities.ReadDatasetHeader(BaseBinaryReader);
            //ReadAndVerifyFiletypeHeader();

            ReadBlobMetadata();
            //ReadFiletypeMetadata();
            // TODO: fix (it has to be updated in wrapping reader after reading metadata)
            MarkDataStartOffset();
        }

        public override void Dispose()
        {
            ((IDisposable)BaseBinaryReader).Dispose();
        }

        public void MarkDataStartOffset()
        {
            DataStartOffset = BaseBinaryReader.BaseStream.Position;
        }

        public byte[] ReadByteBlob(int blobId)
        {
            long position = DataStartOffset + blobId * (long)BlobLength;
            lock (_lockObject)
            {
                SeekIfNeccessary(position);
                return BaseBinaryReader.ReadBytes(BlobLength);
            }
        }

        public float[] ReadFloatBlob(int blobId)
        {
            return DataConversionUtilities.TranslateToFloatArray(ReadByteBlob(blobId));
        }

        public bool[] ReadBoolBlob(int blobId)
        {
            return DataConversionUtilities.TranslateToBoolArray(ReadByteBlob(blobId));
        }

        private void SeekIfNeccessary(long position)
        {
            if (position != BaseBinaryReader.BaseStream.Position)
            {
                BaseBinaryReader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
        }
        

        //private void ReadAndVerifyFiletypeHeader()
        //{
        //    string filetypeId = BaseBinaryReader.ReadString();
        //    if (!filetypeId.Equals(FIXED_SIZE_BLOBS_FILETYPE_ID))
        //    {
        //        throw new IOException($"Fixed-size blob filetype mismatch: {filetypeId}" 
        //            + $" ({FIXED_SIZE_BLOBS_FILETYPE_ID} expected)");
        //    }

        //    int filetypeVersion = BaseBinaryReader.ReadInt32();
        //    if (!filetypeVersion.Equals(FIXED_SIZE_BLOBS_VERSION))
        //    {
        //        throw new IOException($"Fixed-size blob version mismatch: {filetypeVersion}"
        //            + $" ({FIXED_SIZE_BLOBS_VERSION} expected)");
        //    }
        //}
        
        private void ReadBlobMetadata()
        {
            //int metadataSize = BaseBinaryReader.ReadInt32();
            //byte[] blobMetadata = BaseBinaryReader.ReadBytes(metadataSize);

            //using (MemoryStream metadataStream = new MemoryStream(blobMetadata))
            //using (BinaryReader reader = new BinaryReader(metadataStream))
            //{
            //    BlobCount = reader.ReadInt32();
            //    BlobLength = reader.ReadInt32();
            //}
            BlobCount = BaseBinaryReader.ReadInt32();
            BlobLength = BaseBinaryReader.ReadInt32();

            CheckDataFormat();
        }

        private void CheckDataFormat()
        {
            FileFormatUtilities.CheckValueInRange("BlobCount", BlobCount, 1, BLOBCOUNT_LIMIT);
            FileFormatUtilities.CheckValueInRange("BlobLength", BlobLength, 1, BLOBLENGTH_LIMIT);
            
            try
            {
                long totalBytes = (long)BlobCount * BlobLength;
                FileFormatUtilities.CheckValueInRange($"Total data ({BlobCount} count) x ({BlobLength} bytes) == ", totalBytes, 1, FILEOFFSET_LIMIT);
            }
            catch (OverflowException)
            {
                throw new InvalidDataException($"Total data ({BlobCount} count) x ({BlobLength} bytes) caused an overflow.");
            }
        }

        //private void ReadFiletypeMetadata()
        //{
        //    int metadataLength = BaseBinaryReader.ReadInt32();
        //    FiletypeMetadata = BaseBinaryReader.ReadBytes(metadataLength);
        //}
    }
}
