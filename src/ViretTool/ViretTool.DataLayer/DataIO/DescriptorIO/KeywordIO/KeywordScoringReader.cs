using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public class KeywordScoringReader : KeywordScoringIOBase
    {
        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        public byte[] DatasetHeader => BaseBlobReader.DatasetHeader;

        public int ScoringCount => BaseBlobReader.BlobCount;
        public int ScoringVectorSize => BaseBlobReader.BlobLength / sizeof(float);

        public int[] IdToSynsetIdMapping { get; private set; }
        public Dictionary<int, int> SynsetIdToIdMapping { get; private set; }

        public KeywordScoringReader(string filePath)
        {
            BaseBlobReader = new FixedSizeBlobReader(filePath);

            IdToSynsetIdMapping = new int[ScoringCount];
            SynsetIdToIdMapping = new Dictionary<int, int>();

            byte[] metadata = BaseBlobReader.FiletypeMetadata;
            using (BinaryReader reader = new BinaryReader(new MemoryStream(metadata)))
            {
                // id -> synsetId mapping
                for (int i = 0; i < ScoringCount; i++)
                {
                    int synsetId = reader.ReadInt32();
                    IdToSynsetIdMapping[i] = synsetId;
                    SynsetIdToIdMapping[synsetId] = i;
                }
            }
        }

        public override void Dispose()
        {
            BaseBlobReader.Dispose();
        }

        // TODO: consider adding an option to read a subset of values from the array
        // (e.g. only first 5 items of the result float[] vector)
        public float[] ReadScoring(int synsetId)
        {
            return BaseBlobReader.ReadFloatBlob(synsetId);
        }
    }
}
