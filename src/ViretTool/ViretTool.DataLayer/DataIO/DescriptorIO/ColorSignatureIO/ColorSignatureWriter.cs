// TODO: fix format and refactor

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

//namespace ViretTool.DataLayer.DataIO.DescriptorIO.ColorSignatureIO
//{
//    public class ColorSignatureWriter : ColorSignatureIOBase
//    {
//        public FixedSizeBlobWriter BaseBlobWriter { get; private set; }
//        public byte[] DatasetHeader => BaseBlobWriter.DatasetHeader;


//        public ColorSignatureWriter(string outputFile, byte[] datasetHeader,
//            int signatureWidth, int signatureHeight, int signatureCount)
//        {
//            byte[] signaturesMetadata = null;
//            int blobLength = 3 * signatureWidth * signatureHeight; // 3 bytes per Color

//            using (MemoryStream metadataStream = new MemoryStream())
//            using (BinaryWriter writer = new BinaryWriter(metadataStream))
//            {
//                writer.Write(COLOR_SIGNATURES_FILETYPE_ID);
//                writer.Write(COLOR_SIGNATURES_VERSION);

//                writer.Write(signatureWidth);
//                writer.Write(signatureHeight);

//                //writer.Write(signatureCount);
//                //writer.Write(blobLength);


//                // reserve space
//                writer.Write(new byte[METADATA_RESERVE_SPACE_SIZE]);

//                signaturesMetadata = metadataStream.ToArray();
//            }
            
//            BaseBlobWriter = new FixedSizeBlobWriter(
//                outputFile, datasetHeader, signatureCount, blobLength, signaturesMetadata);
//        }
        
//        public override void Dispose()
//        {
//            BaseBlobWriter.Dispose();
//        }


//        public void WriteDescriptor(byte[] signatureData)
//        {
//            BaseBlobWriter.WriteBlob(signatureData);
//        }
//    }
//}
