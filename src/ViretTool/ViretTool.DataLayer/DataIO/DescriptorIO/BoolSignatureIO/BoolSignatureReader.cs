using System.IO;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO
{
    /// <summary>
    /// Reader for bool signature data using an underlying FixedSizeBlobReader.
    /// A bool signature is a 2D bit mask (boolean) where true values (number 1, white color) 
    /// mark regions containing a feature (text, face, etc).
    /// </summary>
    public class BoolSignatureReader : BoolSignatureIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        
        public int DescriptorCount => BaseBlobReader.BlobCount;
        public int DescriptorLength => BaseBlobReader.BlobLength;

        public int SignatureWidth { get; }
        public int SignatureHeight { get; }


        public BoolSignatureReader(string filePath)
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

        public bool[] ReadDescriptor(int id)
        {
            return BaseBlobReader.ReadBoolBlob(id);
        }
    }
}
