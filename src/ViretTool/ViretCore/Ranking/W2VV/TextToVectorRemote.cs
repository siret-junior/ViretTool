using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.W2VV
{
    /// <summary>
    /// Expects 3 files:
    /// "server.url" - contains URL to the query server
    /// "*.pca.matrix.bin" - PCA conversion matrix
    /// "*.pca.mean.bin" - PCA mean vector
    /// </summary>
    public class TextToVectorRemote
    {
        public readonly string ServerUrl;
        public readonly int VectorDimension;

        private readonly PcaConversion _pcaConversion;

        public TextToVectorRemote(string inputDirectory, int vectorDimension)
        {
            ServerUrl = File.ReadAllText(Path.Combine(inputDirectory, "server.url"));
            VectorDimension = vectorDimension;

            _pcaConversion = new PcaConversion(inputDirectory, vectorDimension);
        }

        public static TextToVectorRemote FromDirectory(string inputDirectory, string subDirectory, int vectorDimension)
        {
            try
            {
                return new TextToVectorRemote(Path.Combine(inputDirectory, subDirectory), vectorDimension);
            }
            catch
            {
                // TODO: temporarily fail silently
                return null;
            }
        }

        public /* async? */ float[] TextToVector(string[] query)
        {
            float[] vector = new float[VectorDimension]; // TODO: convert query to vector
            vector = _pcaConversion.ApplyPCA(vector);
            return vector;
        }
    }
}
