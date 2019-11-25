// TODO: fix format and refactor

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

//namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
//{
//    public class SynsetFramesWriter : SynsetFramesIOBase
//    {
//        public FixedSizeBlobWriter BaseBlobWriter { get; private set; }
//        public byte[] DatasetHeader => BaseBlobWriter.DatasetHeader;


//        public SynsetFramesWriter(string outputFile, byte[] datasetHeader,
//            int synsetFramesCount, int synsetCount, int[] idToSynsetIdMapping)
//        {
//            byte[] fileMetadata = null;
//            int blobLength = synsetFramesCount * (sizeof(int) + sizeof(float));

//            using (MemoryStream metadataStream = new MemoryStream())
//            using (BinaryWriter writer = new BinaryWriter(metadataStream))
//            {
//                // id -> synsetId mapping
//                for (int i = 0; i < idToSynsetIdMapping.Length; i++)
//                {
//                    writer.Write(idToSynsetIdMapping[i]);
//                }

//                fileMetadata = metadataStream.ToArray();
//            }

//            BaseBlobWriter = new FixedSizeBlobWriter(
//                outputFile, datasetHeader, synsetCount, blobLength, fileMetadata);
//        }
        
//        public override void Dispose()
//        {
//            BaseBlobWriter.Dispose();
//        }


//        public void WriteSynsetFrames((int frameId, float probability)[] synsetFrames)
//        {
//            byte[] byteBlob = DataConversionUtilities.ConvertToByteArray(synsetFrames);
//            BaseBlobWriter.WriteBlob(byteBlob);
//        }
//    }
//}
