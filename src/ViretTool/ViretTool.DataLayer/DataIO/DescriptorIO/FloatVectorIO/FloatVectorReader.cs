using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO
{
    /// <summary>
    /// Reads an array of float values for each keyframe using an underlaying FixedSizeBlobReader.
    /// The float array is semantically an (array length)-dimensional vector with its length normalized to 1.
    /// </summary>
    public class FloatVectorReader : FloatVectorIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        
        public int DescriptorCount => BaseBlobReader.BlobCount;
        public int DescriptorLength => BaseBlobReader.BlobLength / sizeof(float);
        

        public FloatVectorReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);
        }

        
        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }
        
        public float[] ReadDescriptor(int id)
        {
            return BaseBlobReader.ReadFloatBlob(id);
        }
    }
}
