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
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        public byte[] DatasetHeader => BaseBlobReader.DatasetHeader;

        public int DescriptorCount => BaseBlobReader.BlobCount;
        public int DescriptorLength => BaseBlobReader.BlobLength / sizeof(float);
        
        public string Source { get; }

        //public float[][] Descriptors { get; }


        public FloatVectorReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);
            // not really necessary when no additional metadata is read (but it is wise to keep it here as a reminder)
            BaseBlobReader.MarkDataStartOffset();

            //byte[] metadata = BaseBlobReader.FiletypeMetadata;
            //using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            //{
            //    ReadAndVerifyFiletypeAndVersion(reader);

            //    Source = reader.ReadString();
            //}

            //// load descriptors
            //Descriptors = new float[VectorCount][];
            //for (int iVector = 0; iVector < VectorCount; iVector++)
            //{
            //    Descriptors[iVector] = blobReader.ReadFloatBlob(iVector);
            //}
        }

        
        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }
        
        public float[] ReadDescriptor(int id)
        {
            return BaseBlobReader.ReadFloatBlob(id);
        }


        //private void ReadAndVerifyFiletypeAndVersion(BinaryReader reader)
        //{
        //    string filetype = reader.ReadString();
        //    if (!filetype.Equals(FLOAT_VECTOR_FILETYPE_ID))
        //    {
        //        throw new IOException($"Filetype error: {filetype} (expected {FLOAT_VECTOR_FILETYPE_ID})");
        //    }

        //    int version = reader.ReadInt32();
        //    if (version != FLOAT_VECTOR_VERSION)
        //    {
        //        throw new IOException($"Incorrect \"{FLOAT_VECTOR_FILETYPE_ID}\" filetype version: "
        //            + $"{version} (expected {FLOAT_VECTOR_VERSION})");
        //    }
        //}
    }
}
