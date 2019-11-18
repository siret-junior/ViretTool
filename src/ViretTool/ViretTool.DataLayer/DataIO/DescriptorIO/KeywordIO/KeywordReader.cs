using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.VariableSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public class KeywordReader : KeywordIOBase
    {
        public VariableSizeBlobReader BaseBlobReader { get; private set; }

        private object _lockObject = new object();


        public KeywordReader(string inputFile)
        {
            BaseBlobReader = new VariableSizeBlobReader(inputFile);

            //byte[] metadata = BaseBlobReader.FiletypeMetadata;
            //using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            //{
            //    // TODO if needed    
            //}
        }


        public (int synsetId, float probability)[] ReadSynsets(int frameId)
        {
            lock (_lockObject)
            {
                int blobSizeBytes = BaseBlobReader.GetBlobLength(frameId);
                int synsetCount = blobSizeBytes / (sizeof(int) + sizeof(float)); // synsetId + probability
                BaseBlobReader.SeekToBlob(frameId);
                //int blobSizeBytes2 = BaseBlobReader.BaseBinaryReader.ReadInt32(); // TODO: double check or remove

                List<(int synsetId, float probability)> result = new List<(int synsetId, float probability)>();
                for (int i = 0; i < synsetCount; i++)
                {
                    int synsetId = BaseBlobReader.BaseBinaryReader.ReadInt32();
                    float probability = BaseBlobReader.BaseBinaryReader.ReadSingle();
                    result.Add((synsetId, probability));
                }

                return result.ToArray();
            }
        }


        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }
    }
}
