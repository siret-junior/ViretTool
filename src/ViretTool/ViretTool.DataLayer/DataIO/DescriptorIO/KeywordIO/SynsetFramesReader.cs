using System.Collections.Generic;
using System.IO;
using ViretTool.DataLayer.DataIO.BlobIO.FixedSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    /// <summary>
    /// Reads a small set (typically 5) of tuples (frameId, probability)
    /// for an input keyword (encoded as synsetId)
    /// sorted descending by probability.
    /// Used primarily in example visualizion of the top K results for each keyword.
    /// </summary>
    public class SynsetFramesReader : SynsetFramesIOBase
    {
        private readonly object _lockObject = new object();

        public FixedSizeBlobReader BaseBlobReader { get; private set; }
        
        public int ScoringCount => BaseBlobReader.BlobCount;
        public int ScoringVectorSize => BaseBlobReader.BlobLength / (sizeof(int) + sizeof(float));

        public int[] IdToSynsetIdMapping { get; private set; }
        public Dictionary<int, int> SynsetIdToIdMapping { get; private set; }


        public SynsetFramesReader(string filePath)
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


        public (int frameId, float probability)[] ReadSynsetFrames(int synsetId)
        {
            lock (_lockObject)
            {
                byte[] blob = BaseBlobReader.ReadByteBlob(synsetId);

                using (MemoryStream stream = new MemoryStream(blob))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // one item is tuple of (int) frameId and (float) probability
                    int itemCount = BaseBlobReader.BlobLength / (sizeof(int) + sizeof(float));

                    List<(int frameId, float probability)> result = new List<(int frameId, float probability)>();
                    for (int i = 0; i < itemCount; i++)
                    {
                        int frameId = reader.ReadInt32();
                        float probability = reader.ReadSingle();
                        
                        result.Add((frameId, probability));
                    }

                    return result.ToArray();
                }
            }
        }
    }
}
