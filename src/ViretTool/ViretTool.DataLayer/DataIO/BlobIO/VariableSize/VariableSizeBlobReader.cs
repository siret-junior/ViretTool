using System;
using System.IO;

namespace ViretTool.DataLayer.DataIO.BlobIO.VariableSize
{
    public class VariableSizeBlobReader : VariableSizeBlobIOBase
    {
        public BinaryReader BaseBinaryReader { get; private set; }
        public byte[] DatasetHeader { get; private set; }

        public int BlobCount { get; private set; }
        public long[] BlobOffsets { get; private set; }
        public byte[] FiletypeMetadata { get; private set; }

        private object _lockObject = new object();


        public VariableSizeBlobReader(string filePath)
        {
            BaseBinaryReader = new BinaryReader(
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            DatasetHeader = FileHeaderUtilities.ReadDatasetHeader(BaseBinaryReader);
            ReadAndVerifyFiletypeHeader();
            ReadBlobOffsets();
            ReadBlobMetadata();
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
            if (!filetypeId.Equals(VARIABLE_SIZE_BLOB_FILETYPE_ID))
            {
                throw new IOException($"Fixed-size blob filetype mismatch: {filetypeId}" 
                    + $" ({VARIABLE_SIZE_BLOB_FILETYPE_ID} expected)");
            }

            int filetypeVersion = BaseBinaryReader.ReadInt32();
            if (!filetypeVersion.Equals(VARIABLE_SIZE_BLOB_VERSION))
            {
                throw new IOException($"Fixed-size blob version mismatch: {filetypeVersion}"
                    + $" ({VARIABLE_SIZE_BLOB_VERSION} expected)");
            }
        }
        
        private void ReadBlobOffsets()
        {
            BlobCount = BaseBinaryReader.ReadInt32();
            BlobOffsets = new long[BlobCount];
            for (int i = 0; i < BlobCount; i++)
            {
                BlobOffsets[i] = BaseBinaryReader.ReadInt64();
            }
        }

        private void ReadBlobMetadata()
        {
            int metadataLength = BaseBinaryReader.ReadInt32();
            FiletypeMetadata = BaseBinaryReader.ReadBytes(metadataLength);
        }


    }
}
