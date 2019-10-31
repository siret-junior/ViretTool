using System;
using System.IO;

namespace ViretTool.DataLayer.DataIO.BlobIO.FixedSize
{
    /// <summary>
    /// Lower level file format reader that is used to read binary blobs of fixed size.
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
    public class FixedSizeBlobReader : FizedSizeBlobIOBase
    {
        // debug limits (values are not expected to exceed these limits)
        private const int BLOBLENGTH_LIMIT = 100_000;
        private const int BLOBCOUNT_LIMIT = 10_000_000;
        private const long FILEOFFSET_LIMIT = 1_000_000_000_000;

        /// <summary>
        /// Underlying binary reader used for reading binary data.
        /// </summary>
        public BinaryReader BaseBinaryReader { get; private set; }

        /// <summary>
        /// Offset (position) in the binary file that marks where metadata ends and blob data starts.
        /// Used as a starting point in computing offsets for individual blobs.
        /// </summary>
        public long DataStartOffset { get; private set; }
        
        /// <summary>
        /// Number of stored blobs. 
        /// </summary>
        public int BlobCount { get; private set; }
        
        /// <summary>
        /// Length of each blob in bytes.
        /// </summary>
        public int BlobLength { get; private set; }
        
        /// <summary>
        /// Filetype specific metadata that is interpreted by wrapping reader.
        /// </summary>
        public byte[] FiletypeMetadata { get; private set; }

        /// <summary>
        /// Lock object used when performing reading operations.
        /// Allows concurrent usage of the reader.
        /// </summary>
        private readonly object _lockObject = new object();


        /// <summary>
        /// Constructor opening the underlying binary reader, reading file header (metadata)
        /// and marking the start offset of blob data.
        /// </summary>
        /// <param name="filePath">Path to the input file.</param>
        public FixedSizeBlobReader(string filePath)
        {
            BaseBinaryReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));

            ReadBlobMetadata();
            ReadFiletypeMetadata();

            DataStartOffset = BaseBinaryReader.BaseStream.Position;
        }

        public override void Dispose()
        {
            ((IDisposable)BaseBinaryReader).Dispose();
        }

        /// <summary>
        /// Reads specified blob by computing and seeking to its offset in the file.
        /// </summary>
        /// <param name="blobId">ID (position) of the blob.</param>
        /// <returns>Blob as byte array.</returns>
        public byte[] ReadByteBlob(int blobId)
        {
            if (blobId < 0 || blobId >= BlobCount)
            {
                throw new ArgumentOutOfRangeException($"Trying to read blob ID {blobId} out of range [{0}, {BlobCount - 1}].");
            }

            long position = DataStartOffset + blobId * (long)BlobLength;
            lock (_lockObject)
            {
                SeekIfNeccessary(position);
                return BaseBinaryReader.ReadBytes(BlobLength);
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
        /// Reads the specified byte blob and converts it to bool array.
        /// Bool values are stored as bytes.
        /// </summary>
        /// <param name="blobId">ID of the blob.</param>
        /// <returns>Blob as bool array.</returns>
        public bool[] ReadBoolBlob(int blobId)
        {
            return DataConversionUtilities.ConvertToBoolArray(ReadByteBlob(blobId));
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
        /// Reads fixed size blob metadata (blob count and blob length).
        /// </summary>
        private void ReadBlobMetadata()
        {
            BlobCount = BaseBinaryReader.ReadInt32();
            BlobLength = BaseBinaryReader.ReadInt32();

            CheckBlobMetadata();
        }

        /// <summary>
        /// Checks whether fixed size blob metadata (blob count and blob length)
        /// fall into expected ranges of values.
        /// </summary>
        private void CheckBlobMetadata()
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
