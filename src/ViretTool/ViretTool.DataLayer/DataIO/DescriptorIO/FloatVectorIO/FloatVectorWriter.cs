// TODO: fix format and refactor

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

//namespace ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO
//{
//    public class FloatVectorWriter : FloatVectorIOBase
//    {
//        public FixedSizeBlobWriter BaseBlobWriter { get; private set; }
//        public byte[] DatasetHeader => BaseBlobWriter.DatasetHeader;


//        public FloatVectorWriter(string outputFile, byte[] datasetHeader,
//            int vectorLength, int vectorCount, string source = "NO SOURCE PROVIDED")
//        {
//            byte[] vectorMetadata = null;
//            int blobLength = vectorLength * sizeof(float);

//            using (MemoryStream metadataStream = new MemoryStream())
//            using (BinaryWriter writer = new BinaryWriter(metadataStream))
//            {
//                writer.Write(FLOAT_VECTOR_FILETYPE_ID);
//                writer.Write(FLOAT_VECTOR_VERSION);

//                writer.Write(source);

//                // reserve space
//                writer.Write(new byte[METADATA_RESERVE_SPACE_SIZE]);

//                vectorMetadata = metadataStream.ToArray();
//            }

//            BaseBlobWriter = new FixedSizeBlobWriter(
//                outputFile, datasetHeader, vectorCount, blobLength, vectorMetadata);
//        }

//        public override void Dispose()
//        {
//            BaseBlobWriter.Dispose();
//        }

//        public void WriteDescriptor(float[] floatVector)
//        {
//            byte[] byteBlob = DataConversionUtilities.ConvertToByteArray(floatVector);
//            BaseBlobWriter.WriteBlob(byteBlob);
//        }
//    }
//}
