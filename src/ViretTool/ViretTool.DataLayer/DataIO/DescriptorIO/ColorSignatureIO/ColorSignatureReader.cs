using System.IO;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO
{
    /// <summary>
    /// Reads color signatures from an input file.
    /// Color signature is a 2D bitmap where color of each pixel corresponds 
    /// to a dominant color of a region in the source frame image.
    /// </summary>
    public class ColorSignatureReader : ColorSignatureIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        
        public int DescriptorCount => BaseBlobReader.BlobCount;
        public int DescriptorLength => BaseBlobReader.BlobLength;

        public int SignatureWidth { get; }
        public int SignatureHeight { get; }


        public ColorSignatureReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);

            byte[] metadata = BaseBlobReader.FiletypeMetadata;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            {
                SignatureWidth = reader.ReadInt32();
                SignatureHeight = reader.ReadInt32();
            }
        }

        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }

        public byte[] ReadDescriptor(int id)
        {
            return BaseBlobReader.ReadByteBlob(id);
        }
    }
}
