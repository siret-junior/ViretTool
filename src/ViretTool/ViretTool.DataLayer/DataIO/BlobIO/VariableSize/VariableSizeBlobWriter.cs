using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.BlobIO.VariableSize
{
    public class VariableSizeBlobWriter : VariableSizeBlobIOBase
    {
        public BinaryWriter BaseBinaryWriter { get; private set; }
        private object _lockObject = new object();


        // common for all files extracted from the same dataset at the same time
        public byte[] DatasetHeader { get; private set; }

        // blob filetype specific
        public int BlobCount { get; private set; }
        public long[] BlobOffsets { get; private set; }
        public int[] BlobLengths { get; private set; }

        // blob interpretation specific
        public byte[] FiletypeMetadata { get; private set; }

        private int _writtenBlobCount = 0;
        private long _blobOffsetsStreamOffset = -1;
        private long _blobLengthsStreamOffset = -1;

        public VariableSizeBlobWriter(string outputFile, byte[] datasetHeader, int blobCount, byte[] filetypeMetadata)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            BaseBinaryWriter = new BinaryWriter(
                File.Open(outputFile, FileMode.CreateNew, FileAccess.Write, FileShare.None));
            
            //**** dataset header
            //DatasetHeader = datasetHeader;
            //BaseBinaryWriter.Write(datasetHeader.Length);
            //BaseBinaryWriter.Write(datasetHeader);

            //**** blob header
            //BaseBinaryWriter.Write(VARIABLE_SIZE_BLOBS_FILETYPE_ID);
            //BaseBinaryWriter.Write(VARIABLE_SIZE_BLOBS_VERSION);

            //**** blob metadata
            // metadata length placeholder
            int metadataLength = -1;
            long metadataLengthStreamOffset = BaseBinaryWriter.BaseStream.Position;
            BaseBinaryWriter.Write(metadataLength);

            BlobCount = blobCount;
            BaseBinaryWriter.Write(blobCount);

            BlobOffsets = new long[BlobCount];
            _blobOffsetsStreamOffset = BaseBinaryWriter.BaseStream.Position;
            BaseBinaryWriter.BaseStream.Seek(BlobCount * sizeof(long), SeekOrigin.Current);

            BlobLengths = new int[BlobCount];
            _blobLengthsStreamOffset = BaseBinaryWriter.BaseStream.Position;
            BaseBinaryWriter.BaseStream.Seek(BlobCount * sizeof(int), SeekOrigin.Current);

            // reserve space
            //BaseBinaryWriter.Seek(METADATA_RESERVE_SPACE_SIZE, SeekOrigin.Current);

            // rewrite metadata size placeholder
            long currentOffset = BaseBinaryWriter.BaseStream.Position;
            BaseBinaryWriter.BaseStream.Seek(metadataLengthStreamOffset, SeekOrigin.Begin);
            BaseBinaryWriter.Write((int)(currentOffset - metadataLengthStreamOffset - sizeof(int)));
            BaseBinaryWriter.BaseStream.Seek(currentOffset, SeekOrigin.Begin);

            //**** Filetype metadata
            FiletypeMetadata = filetypeMetadata;
            BaseBinaryWriter.Write(filetypeMetadata.Length);
            BaseBinaryWriter.Write(filetypeMetadata);
        }

        public void WriteBlob(byte[] blob)
        {
            if (_writtenBlobCount >= BlobCount)
            {
                throw new ArgumentOutOfRangeException(
                    $"Trying to write more blobs than previously reserved: {BlobCount}");
            }

            BlobOffsets[_writtenBlobCount] = BaseBinaryWriter.BaseStream.Position;
            BlobLengths[_writtenBlobCount] = blob.Length;
            BaseBinaryWriter.Write(blob.Length);
            BaseBinaryWriter.Write(blob);

            _writtenBlobCount++;
        }


        public override void Dispose()
        {
            // rewrite placeholders
            BaseBinaryWriter.BaseStream.Seek(_blobOffsetsStreamOffset, SeekOrigin.Begin);
            for (int iBlob = 0; iBlob < BlobCount; iBlob++)
            {
                BaseBinaryWriter.Write(BlobOffsets[iBlob]);
            }

            BaseBinaryWriter.BaseStream.Seek(_blobLengthsStreamOffset, SeekOrigin.Begin);
            for (int iBlob = 0; iBlob < BlobCount; iBlob++)
            {
                BaseBinaryWriter.Write(BlobLengths[iBlob]);
            }

            BaseBinaryWriter.Dispose();
        }
    }
}
