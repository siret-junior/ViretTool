using System;
using System.IO;

namespace ViretTool.DataLayer.DataIO.BlobIO.VariableSize
{
    public class VariableSizeBlobReader : VariableSizeBlobIOBase
    {
        public BinaryReader BaseBinaryReader { get; private set; }
        private object _lockObject = new object();

        // common for all files extracted from the same dataset at the same time
        public byte[] DatasetHeader { get; private set; }

        // blob specific metadata
        public int BlobCount { get; private set; }
        public long[] BlobOffsets { get; private set; }
        public int[] BlobLengths { get; private set; }

        // blob filetype interpretation metadata
        public byte[] FiletypeMetadata { get; private set; }

        

        public VariableSizeBlobReader(string filePath)
        {
            BaseBinaryReader = new BinaryReader(
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            DatasetHeader = FileHeaderUtilities.ReadDatasetHeader(BaseBinaryReader);
            ReadAndVerifyFiletypeHeader();
            
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
                int blobLength = BaseBinaryReader.ReadInt32();
                return BaseBinaryReader.ReadBytes(blobLength);
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

        
        private void ReadAndVerifyFiletypeHeader()
        {
            string filetypeId = BaseBinaryReader.ReadString();
            if (!filetypeId.Equals(VARIABLE_SIZE_BLOBS_FILETYPE_ID))
            {
                throw new IOException($"Fixed-size blob filetype mismatch: {filetypeId}" 
                    + $" ({VARIABLE_SIZE_BLOBS_FILETYPE_ID} expected)");
            }

            int filetypeVersion = BaseBinaryReader.ReadInt32();
            if (!filetypeVersion.Equals(VARIABLE_SIZE_BLOBS_VERSION))
            {
                throw new IOException($"Fixed-size blob version mismatch: {filetypeVersion}"
                    + $" ({VARIABLE_SIZE_BLOBS_VERSION} expected)");
            }
        }
        
        private void ReadBlobMetadata()
        {
            int metadataLength = BaseBinaryReader.ReadInt32();
            byte[] blobMetadata = BaseBinaryReader.ReadBytes(metadataLength);

            using (MemoryStream metadataStream = new MemoryStream(blobMetadata))
            using (BinaryReader reader = new BinaryReader(metadataStream))
            {
                BlobCount = reader.ReadInt32();
                BlobOffsets = new long[BlobCount];
                for (int i = 0; i < BlobCount; i++)
                {
                    BlobOffsets[i] = reader.ReadInt64();
                }
                BlobLengths = new int[BlobCount];
                for (int i = 0; i < BlobCount; i++)
                {
                    BlobLengths[i] = reader.ReadInt32();
                }
            }    
        }

        private void ReadFiletypeMetadata()
        {
            int metadataLength = BaseBinaryReader.ReadInt32();
            FiletypeMetadata = BaseBinaryReader.ReadBytes(metadataLength);
        }
    }
}
