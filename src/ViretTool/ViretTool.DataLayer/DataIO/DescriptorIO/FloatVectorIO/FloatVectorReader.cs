using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO
{
    public class FloatVectorReader : FloatVectorIOBase
    {
        public byte[] DatasetHeader { get; }

        public int VectorCount { get; }
        public int VectorLength { get; }

        public string Source { get; }

        public float[][] Descriptors { get; }


        public FloatVectorReader(string filePath)
        {
            using (FixedSizeBlobReader blobReader = new FixedSizeBlobReader(filePath))
            {
                DatasetHeader = blobReader.DatasetHeader;
                VectorCount = blobReader.BlobCount;
                VectorLength = blobReader.BlobLength / sizeof(float);

                // load metadata
                byte[] metadata = blobReader.FiletypeMetadata;
                using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
                {
                    Source = reader.ReadString();
                }

                // load descriptors
                Descriptors = new float[VectorCount][];
                for (int iVector = 0; iVector < VectorCount; iVector++)
                {
                    Descriptors[iVector] = blobReader.ReadFloatBlob(iVector);
                }
            }
        }
    }
}
