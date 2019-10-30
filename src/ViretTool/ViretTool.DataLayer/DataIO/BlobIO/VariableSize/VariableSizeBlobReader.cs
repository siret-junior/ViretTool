using System;
using System.IO;

namespace ViretTool.DataLayer.DataIO.BlobIO.VariableSize
{
    // TODO: document blob size limitations (expectations for format checking)
    public class VariableSizeBlobReader : VariableSizeBlobIOBase
    {
        public BinaryReader BaseBinaryReader { get; private set; }
        // common for all files extracted from the same dataset at the same time
        public byte[] DatasetHeader { get; private set; }

        // blob specific metadata
        public int BlobCount { get; private set; }
        public long[] BlobOffsets { get; private set; }
        public int[] BlobLengths { get; private set; }

        // blob filetype interpretation metadata
        public byte[] FiletypeMetadata { get; private set; }

        private object _lockObject = new object();

        private const int BLOBLENGTH_LIMIT = 100_000;
        private const int BLOBCOUNT_LIMIT = 10_000_000;
        private const long FILEOFFSET_LIMIT = 1_000_000_000_000;
        

        public VariableSizeBlobReader(string filePath)
        {
            BaseBinaryReader = new BinaryReader(
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            //DatasetHeader = FileHeaderUtilities.ReadDatasetHeader(BaseBinaryReader);
            //ReadAndVerifyFiletypeHeader();
            
            ReadBlobMetadata();
            ReadFiletypeMetadata();
        }

        public override void Dispose()
        {
            ((IDisposable)BaseBinaryReader).Dispose();
        }


        public byte[] ReadByteBlob(int blobId)
        {
            long position = BlobOffsets[blobId];
            lock (_lockObject)
            {
                SeekIfNeccessary(position);
                //int blobLength = BaseBinaryReader.ReadInt32();
                //if (blobLength != GetBlobSize(blobId))
                //{
                //    throw new InvalidDataException("VariableSize blob lengths are not equal!");
                //}
                return BaseBinaryReader.ReadBytes(GetBlobSize(blobId));
            }
        }

        public float[] ReadFloatBlob(int blobId)
        {
            return DataConversionUtilities.TranslateToFloatArray(ReadByteBlob(blobId));
        }

        public void SeekToBlob(int blobId)
        {
            long position = BlobOffsets[blobId];
            lock (_lockObject)
            {
                SeekIfNeccessary(position);
            }
        }

        public int GetBlobSize(int blobId)
        {
            return BlobLengths[blobId];
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
        //    if (!filetypeId.Equals(VARIABLE_SIZE_BLOBS_FILETYPE_ID))
        //    {
        //        throw new IOException($"Fixed-size blob filetype mismatch: {filetypeId}" 
        //            + $" ({VARIABLE_SIZE_BLOBS_FILETYPE_ID} expected)");
        //    }

        //    int filetypeVersion = BaseBinaryReader.ReadInt32();
        //    if (!filetypeVersion.Equals(VARIABLE_SIZE_BLOBS_VERSION))
        //    {
        //        throw new IOException($"Fixed-size blob version mismatch: {filetypeVersion}"
        //            + $" ({VARIABLE_SIZE_BLOBS_VERSION} expected)");
        //    }
        //}
        
        private void ReadBlobMetadata()
        {
            //int metadataLength = BaseBinaryReader.ReadInt32();
            //byte[] blobMetadata = BaseBinaryReader.ReadBytes(metadataLength);

            //using (MemoryStream metadataStream = new MemoryStream(blobMetadata))
            //using (BinaryReader reader = new BinaryReader(metadataStream))
            BinaryReader reader = BaseBinaryReader;
            {
                BlobCount = reader.ReadInt32();
                FileFormatUtilities.CheckValueInRange("BlobCount", BlobCount, 1, BLOBCOUNT_LIMIT);

                BlobOffsets = new long[BlobCount];
                for (int i = 0; i < BlobCount; i++)
                {
                    BlobOffsets[i] = reader.ReadInt64();
                }
                FileFormatUtilities.CheckValuesInRange("BlobOffsets", BlobOffsets, 1, FILEOFFSET_LIMIT);
                FileFormatUtilities.CheckValuesIncrement("BlobOffsets", BlobOffsets);

                BlobLengths = new int[BlobCount];
                for (int i = 0; i < BlobCount; i++)
                {
                    BlobLengths[i] = reader.ReadInt32();
                }
                FileFormatUtilities.CheckValuesInRange("BlobLengths", BlobLengths, 0, BLOBLENGTH_LIMIT);
            }
        }


        private void ReadFiletypeMetadata()
        {
            int metadataLength = BaseBinaryReader.ReadInt32();
            FiletypeMetadata = BaseBinaryReader.ReadBytes(metadataLength);
        }
    }
}
