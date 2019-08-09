using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public class KeywordScoringWriter : KeywordScoringIOBase
    {
        public FixedSizeBlobWriter BaseBlobWriter { get; private set; }
        public byte[] DatasetHeader => BaseBlobWriter.DatasetHeader;


        public KeywordScoringWriter(string outputFile, byte[] datasetHeader,
            int scoringVectorSize, int scoringCount, int[] idToSynsetIdMapping)
        {
            byte[] fileMetadata = null;
            int blobLength = scoringVectorSize * sizeof(float);

            using (MemoryStream metadataStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(metadataStream))
            {
                // id -> synsetId mapping
                for (int i = 0; i < idToSynsetIdMapping.Length; i++)
                {
                    writer.Write(idToSynsetIdMapping[i]);
                }
                
                fileMetadata = metadataStream.ToArray();
            }

            BaseBlobWriter = new FixedSizeBlobWriter(
                outputFile, datasetHeader, scoringCount, blobLength, fileMetadata);
        }

        public override void Dispose()
        {
            BaseBlobWriter.Dispose();
        }


        public void WriteScoring(float[] scoringVector)
        {
            byte[] byteBlob = DataConversionUtilities.TranslateToByteArray(scoringVector);
            BaseBlobWriter.WriteBlob(byteBlob);
        }
    }
}
