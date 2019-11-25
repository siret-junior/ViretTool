using System.Collections.Generic;
using ViretTool.DataLayer.DataIO.BlobIO.VariableSize;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    /// <summary>
    /// Reads top K (typically 10) tuples of (synsetId, probability) for each input keyframe.
    /// Used primarily in visualization of keyword predictions for each keyframe.
    /// </summary>
    public class FrameSynsetsReader : FrameSynsetsIOBase
    {
        public VariableSizeBlobReader BaseBlobReader { get; private set; }

        private readonly object _lockObject = new object();


        public FrameSynsetsReader(string inputFile)
        {
            BaseBlobReader = new VariableSizeBlobReader(inputFile);
        }


        public (int synsetId, float probability)[] ReadSynsets(int frameId)
        {
            lock (_lockObject)
            {
                // one item is tuple of (int) synsetId and (float) probability
                int blobLengthBytes = BaseBlobReader.GetBlobLength(frameId);
                int tupleCount = blobLengthBytes / (sizeof(int) + sizeof(float));
                
                BaseBlobReader.SeekToBlob(frameId);
                List<(int synsetId, float probability)> result = new List<(int synsetId, float probability)>();
                for (int i = 0; i < tupleCount; i++)
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
