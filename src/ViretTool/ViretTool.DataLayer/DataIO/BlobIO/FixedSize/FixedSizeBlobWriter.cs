using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.BlobIO.FixedSize
{
    public class FixedSizeBlobWriter : FizedSizeBlobIOBase
    {
        public BinaryWriter BaseBinaryWriter { get; private set; }
        public byte[] DatasetHeader { get; private set; }
        
        public int BlobCount { get; private set; }
        public int BlobLength { get; private set; }
        public byte[] FiletypeMetadata { get; private set; }

        private int _writtenBlobCount = 0;

        public FixedSizeBlobWriter(string filePath, byte[] datasetHeader, int blobCount, int blobLength, 
            byte[] filetypeMetadata = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            BaseBinaryWriter = new BinaryWriter(
                File.Open(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None));

            //**** dataset header
            DatasetHeader = datasetHeader;
            BaseBinaryWriter.Write(datasetHeader.Length);
            BaseBinaryWriter.Write(datasetHeader);

            //**** blob header
            BaseBinaryWriter.Write(FIXED_SIZE_BLOBS_FILETYPE_ID);
            BaseBinaryWriter.Write(FIXED_SIZE_BLOBS_VERSION);

            //**** blob metadata
            // metadata length placeholder
            int metadataLength = -1;
            long metadataLengthStreamOffset = BaseBinaryWriter.BaseStream.Position;
            BaseBinaryWriter.Write(metadataLength);

            BlobCount = blobCount;
            BaseBinaryWriter.Write(blobCount);

            BlobLength = blobLength;
            BaseBinaryWriter.Write(blobLength);

            // reserve space
            BaseBinaryWriter.Seek(METADATA_RESERVE_SPACE_SIZE, SeekOrigin.Current);

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
        
        public override void Dispose()
        {
            BaseBinaryWriter.Dispose();
        }

        public void WriteBlob(byte[] blob)
        {
            if (_writtenBlobCount >= BlobCount)
            {
                throw new ArgumentOutOfRangeException(
                    $"Trying to write more blobs than previously reserved: {BlobCount}.");
            }
            if (blob.Length != BlobLength)
            {
                throw new ArgumentOutOfRangeException(
                    $"Trying to write a blob with incorrect length: {blob.Length} (expected {BlobLength}).");
            }
            
            BaseBinaryWriter.Write(blob);
            _writtenBlobCount++;
        }

    }
}
