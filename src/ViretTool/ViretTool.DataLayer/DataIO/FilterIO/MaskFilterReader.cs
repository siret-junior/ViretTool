using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.FilterIO
{
    public class MaskFilterReader : MaskFilterIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        public byte[] DatasetHeader => BaseBlobReader.DatasetHeader;
        public int FrameCount => BaseBlobReader.BlobLength / sizeof(float);
        

        public MaskFilterReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);

            byte[] metadata = BaseBlobReader.FiletypeMetadata;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            {
                ReadAndVerifyFiletypeAndVersion(reader);
            }
        }


        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }

        public static float[] ReadFilter(string inputFile)
        {
            using (MaskFilterReader reader = new MaskFilterReader(inputFile))
            {
                return reader.ReadFilter();
            }
        }


        public float[] ReadFilter()
        {
            // there is only one blob with all filter values
            return BaseBlobReader.ReadFloatBlob(0);
        }

        
        private void ReadAndVerifyFiletypeAndVersion(BinaryReader reader)
        {
            string filetype = reader.ReadString();
            if (!filetype.Equals(MASK_FILTER_FILETYPE_ID))
            {
                throw new IOException($"Filetype error: {filetype} (expected {MASK_FILTER_FILETYPE_ID})");
            }

            int version = reader.ReadInt32();
            if (version != MASK_FILTER_VERSION)
            {
                throw new IOException($"Incorrect \"{MASK_FILTER_FILETYPE_ID}\" filetype version: "
                    + $"{version} (expected {MASK_FILTER_VERSION})");
            }
        }

    }
}
