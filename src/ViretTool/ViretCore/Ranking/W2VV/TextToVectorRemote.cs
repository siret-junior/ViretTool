using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public /* async? */ float[] TextToVector(string[] queryKeywords)
        {
            if (queryKeywords == null || queryKeywords.Length == 0)
            {
                float[] vector = new float[VectorDimension]; // TODO: convert query to vector
                vector = _pcaConversion.ApplyPCA(vector);
                return vector;
            }

            // build url
            // TODO: add actual server url
            // --- HERE ADD ACTUAL SERVER URL ---//
            string serverUrl = "http://195.113.18.36:42013/";
            string service = "clip/"; // change to "bert/" if BERT is needed
            string queryUrlEncoded = Uri.EscapeDataString(string.Join(" ", queryKeywords)); // my%20text%20query 
            string url = serverUrl + service + queryUrlEncoded;

            // get data
            byte[] responseDataBytes;
            using (WebClient webClient = new WebClient())
            {
                // TODO: Endianness?
                responseDataBytes = webClient.DownloadData(url);
                if (responseDataBytes.Length != VectorDimension * sizeof(float))
                {
                    throw new InvalidDataException($"Remote text-to-vector service returned vector bytes of incorrect length."
                        + $"Expected: {VectorDimension * sizeof(float)}, received: {responseDataBytes.Length}.");
                }
            }

            // convert to floats
            float[] responseVectorFloats = new float[VectorDimension];
            Buffer.BlockCopy(responseDataBytes, 0, responseVectorFloats, 0, responseDataBytes.Length);

            // apply PCA
            float[] resultVector = _pcaConversion.ApplyPCA(responseVectorFloats);

            return resultVector;
        }
    }
}
