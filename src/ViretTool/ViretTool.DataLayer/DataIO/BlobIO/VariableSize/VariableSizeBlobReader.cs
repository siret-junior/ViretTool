using System;
using System.IO;

namespace ViretTool.DataLayer.DataIO.BlobIO.VariableSize
{
    /// <summary>
    /// Lower level file format reader that is used to read binary blobs of variable size.
    /// It can be used by higher level reader wrappers to read and interpret stored blobs.
    /// Furthermore, it allows storage of filetype specific metadata that can also be
    /// interpreted by wrapping higher level reader.
    /// 
    /// The reader can be used concurrently.
    /// 
    /// For debug purposes there are imposed some limits that are not expected to be reached.
    /// Reaching those limits trips an exception. The limits are:
    /// - length of a blob is limited to 100 000 bytes.
    /// - count of blobs is limited to 10 000 000.
    /// - limit of file offsets (maximum file length) is limited to 1 000 000 000 000 bytes (approximately 1TB).
    /// </summary>
    public class VariableSizeBlobReader : VariableSizeBlobIOBase
    {
        private const int BLOBLENGTH_LIMIT = 100_000;
        private const int BLOBCOUNT_LIMIT = 100_000_000;
        private const long FILEOFFSET_LIMIT = 1_000_000_000_000;

        /// <summary>
        /// Underlying binary reader used for reading binary data.
        /// </summary>
        public BinaryReader BaseBinaryReader { get; private set; }

        /// <summary>
        /// Number of stored blobs. 
        /// </summary>
        public int BlobCount { get; private set; }
        
        /// <summary>
        /// File stream offsets for each blob.
        /// Because blobs have variable length, we need to store offset of each blob individually.
        /// </summary>
        public long[] BlobOffsets { get; private set; }
        
        /// <summary>
        /// Blob lengths in bytes.
        /// </summary>
        public int[] BlobLengths { get; private set; }

        /// <summary>
        /// Filetype specific metadata that is interpreted by wrapping reader.
        /// </summary>
        public byte[] FiletypeMetadata { get; private set; }

        /// <summary>
        /// Lock object used when performing reading operations.
        /// Allows concurrent usage of the reader.
        /// </summary>
        private object _lockObject = new object();


        /// <summary>
        /// Constructor opening the underlying binary reader and reading file header (metadata).
        /// </summary>
        /// <param name="filePath">Path to the input file.</param>
        public VariableSizeBlobReader(string filePath)
        {
            BaseBinaryReader = new BinaryReader(
                File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            ReadBlobMetadata();
            ReadFiletypeMetadata();
        }

        public override void Dispose()
        {
            ((IDisposable)BaseBinaryReader).Dispose();
        }


        /// <summary>
        /// Reads specified blob by seeking to its offset in the file and reading its number of bytes.
        /// </summary>
        /// <param name="blobId">ID (position) of the blob.</param>
        /// <returns>Blob as byte array.</returns>
        public byte[] ReadByteBlob(int blobId)
        {
            long position = BlobOffsets[blobId];
            lock (_lockObject)
            {
                SeekIfNeccessary(position);
                return BaseBinaryReader.ReadBytes(GetBlobLength(blobId));
            }
        }

        /// <summary>
        /// Reads the specified byte blob and converts it to float array.
        /// </summary>
        /// <param name="blobId">ID of the blob.</param>
        /// <returns>Blob as float array.</returns>
        public float[] ReadFloatBlob(int blobId)
        {
            return DataConversionUtilities.ConvertToFloatArray(ReadByteBlob(blobId));
        }

        /// <summary>
        /// Seeks to the starting offset of the specified blob.
        /// Used when the blob has a complex format that needs to be read differently by wrapping higher level reader.
        /// </summary>
        /// <param name="blobId">ID of the blob.</param>
        public void SeekToBlob(int blobId)
        {
            if (blobId < 0 || blobId >= BlobCount)
            {
                throw new ArgumentOutOfRangeException($"Trying to read blob ID {blobId} out of range [{0}, {BlobCount - 1}].");
            }

            long position = BlobOffsets[blobId];
            lock (_lockObject)
            {
                SeekIfNeccessary(position);
            }
        }

        /// <summary>
        /// Returns length of the specified blob.
        /// </summary>
        /// <param name="blobId">ID of the blob.</param>
        /// <returns>Length of the specified blob in bytes.</returns>
        public int GetBlobLength(int blobId)
        {
            return BlobLengths[blobId];
        }


        /// <summary>
        /// Seeks the underlying binary reader to the desired position if necessary.
        /// </summary>
        /// <param name="position"></param>
        private void SeekIfNeccessary(long position)
        {
            if (position != BaseBinaryReader.BaseStream.Position)
            {
                BaseBinaryReader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Reads variable size blob metadata (blob count, blob offsets and blob lengths).
        /// </summary>
        private void ReadBlobMetadata()
        {
            // blob count
            BlobCount = BaseBinaryReader.ReadInt32();
            FileFormatUtilities.CheckValueInRange("BlobCount", BlobCount, 1, BLOBCOUNT_LIMIT);

            // blob offsets
            BlobOffsets = new long[BlobCount];
            for (int i = 0; i < BlobCount; i++)
            {
                BlobOffsets[i] = BaseBinaryReader.ReadInt64();
            }
            FileFormatUtilities.CheckValuesInRange("BlobOffsets", BlobOffsets, 1, FILEOFFSET_LIMIT);
            FileFormatUtilities.CheckValuesIncrement("BlobOffsets", BlobOffsets);

            // blob lengths
            BlobLengths = new int[BlobCount];
            for (int i = 0; i < BlobCount; i++)
            {
                BlobLengths[i] = BaseBinaryReader.ReadInt32();
            }
            FileFormatUtilities.CheckValuesInRange("BlobLengths", BlobLengths, 0, BLOBLENGTH_LIMIT);
        }

        /// <summary>
        /// Reads filetype specific metadata as a byte array without understanding its meaning.
        /// The filetype specific metadata will be interpreted by wrapping higher level reader.
        /// </summary>
        private void ReadFiletypeMetadata()
        {
            int metadataLength = BaseBinaryReader.ReadInt32();
            FiletypeMetadata = BaseBinaryReader.ReadBytes(metadataLength);
        }
    }
}
