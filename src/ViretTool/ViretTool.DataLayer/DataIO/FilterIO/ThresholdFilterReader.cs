using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.FilterIO
{
    /// <summary>
    /// Reads an array of float values (usually in range 0 to 1) for each keyframe.
    /// Semantically represents quantity of a property (e.g. how saturated/desaturated are frame images).
    /// </summary>
    public class ThresholdFilterReader : ThresholdFilterIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        public int DescriptorCount => BaseBlobReader.BlobCount;
        public int DescriptorLength => BaseBlobReader.BlobLength / sizeof(float);


        public ThresholdFilterReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);
        }


        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }

        public static float[] ReadFilter(string inputFile)
        {
            using (ThresholdFilterReader reader = new ThresholdFilterReader(inputFile))
            {
                return reader.ReadFilter();
            }
        }


        public float[] ReadFilter()
        {
            float[] resultFilter = new float[DescriptorCount];
            for (int i = 0; i < DescriptorCount; i++)
            {
                resultFilter[i] = BaseBlobReader.ReadFloatBlob(i)[0];
            }
            return resultFilter;
        }

        

    }
}
