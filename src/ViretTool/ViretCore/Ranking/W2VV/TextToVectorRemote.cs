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

        private readonly bool _applyPca;
        private readonly PcaConversion _pcaConversion;
        
        public TextToVectorRemote(string serverUrlFile, string pcaMatrixFile, string pcaMeanFile, int vectorDimension)
        {
            ServerUrl = File.ReadAllText(serverUrlFile);
            _pcaConversion = new PcaConversion(pcaMatrixFile, pcaMeanFile, vectorDimension);
            _applyPca = true;
            VectorDimension = vectorDimension;
        }

        public TextToVectorRemote(string serverUrlFile, int vectorDimension)
        {
            ServerUrl = File.ReadAllText(serverUrlFile);
            _applyPca = false;
            VectorDimension = vectorDimension;
        }

        public static TextToVectorRemote FromDirectory(string inputDirectory, string serverUrlPattern,
            string pcaMatrixPattern, string pcaMeanPattern, int vectorDimension)
        {
            if (!Directory.Exists(inputDirectory))
            {
                return null;
            }
            // load filenames based on patterns
            string serverUrlFile = Directory.GetFiles(inputDirectory, serverUrlPattern).FirstOrDefault();
            string pcaMatrixFile = Directory.GetFiles(inputDirectory, pcaMatrixPattern).FirstOrDefault();
            string pcaMeanFile = Directory.GetFiles(inputDirectory, pcaMeanPattern).FirstOrDefault();

            try
            {
                // check if files exist
                foreach ((string file, string pattern) in new (string, string)[]
                {
                    (serverUrlFile, serverUrlPattern),
                    (pcaMatrixFile, pcaMatrixPattern), 
                    (pcaMeanFile, pcaMeanPattern)
                })
                {
                    if (file == null)
                    {
                        throw new FileNotFoundException($"File '{file}' was not found in directory '{inputDirectory}'.");
                    }
                }

                // load the instance
                return new TextToVectorRemote(serverUrlFile, pcaMatrixFile, pcaMeanFile, vectorDimension);
            }
            catch
            {
                // TODO: temporarily fail silently
                return null;
            }
        }

        public static TextToVectorRemote FromDirectory(string inputDirectory, string serverUrlPattern, int vectorDimension)
        {
            try
            {
                // load filenames based on patterns
                string serverUrlFile = Directory.GetFiles(inputDirectory, serverUrlPattern).FirstOrDefault();

                // check if files exist
                foreach ((string file, string pattern) in new (string, string)[]
                {
                    (serverUrlFile, serverUrlPattern),
                })
                {
                    if (file == null)
                    {
                        throw new FileNotFoundException($"File '{file}' was not found in directory '{inputDirectory}'.");
                    }
                }

                // load the instance
                return new TextToVectorRemote(serverUrlFile, vectorDimension);
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
                vector = _applyPca ? _pcaConversion.ApplyPCA(vector) : vector; 
                return vector;
            }

            // build url
            string queryUrlEncoded = Uri.EscapeDataString(string.Join(" ", queryKeywords)); // my%20text%20query 
            string url = ServerUrl + queryUrlEncoded;

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
            float[] resultVector = _applyPca ? _pcaConversion.ApplyPCA(responseVectorFloats) : responseVectorFloats;

            return resultVector;
        }
    }
}
