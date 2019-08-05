using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.VariableSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public class KeywordWriter : KeywordIOBase
    {
        public VariableSizeBlobWriter BaseBlobWriter { get; private set; }
        public byte[] DatasetHeader => BaseBlobWriter.DatasetHeader;


        public KeywordWriter(string outputFile, byte[] datasetHeader,
            int frameCount, string source = "NO SOURCE PROVIDED")
        {
            byte[] keywordMetadata = null;
            int blobLength = frameCount * (sizeof(int) + sizeof(float));

            using (MemoryStream metadataStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(metadataStream))
            {
                writer.Write(source);

                keywordMetadata = metadataStream.ToArray();
            }

            BaseBlobWriter = new VariableSizeBlobWriter(
                outputFile, datasetHeader, frameCount, keywordMetadata);
        }

        public override void Dispose()
        {
            BaseBlobWriter.Dispose();
        }

        public void WriteDescriptor((int synsetId, float probability)[] synsetProbabilities)
        {
            byte[] byteBlob = DataConversionUtilities.TranslateToByteArray(synsetProbabilities);
            BaseBlobWriter.WriteBlob(byteBlob);
        }
    }
}
