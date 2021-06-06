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

        public /* async? */ float[] TextToVector(string[] query)
        {
            if (query.Length < 1)
            {
                float[] vector = new float[VectorDimension]; // TODO: convert query to vector
                vector = _pcaConversion.ApplyPCA(vector);
                return vector;
            }
            //my%20text%20query
            StringBuilder URLBuilder = new StringBuilder();

            // TODO: add actual server url
            // --- HERE ADD ACTUAL SERVER URL ---//
            string serverUrl = "http://195.113.18.36:42013/";

            URLBuilder.Append(serverUrl);

            string service = "clip/"; // change to "bert/" if BERT is needed

            URLBuilder.Append(service);
            // --- END OF "HERE ADD ACTUAL SERVER URL" ---//

            bool firstKeyword = true;
            foreach (string keyword in query)
            {
                if (!firstKeyword)
                    URLBuilder.Append("%20");
                else
                    firstKeyword = false;

                URLBuilder.Append(keyword);
            }

            string url = URLBuilder.ToString();

            WebClient webClient = new WebClient();

            byte[] responseData = webClient.DownloadData(url);

            // TODO: Endianness?
            float[] responseVector = new float[VectorDimension];

            // any more elegant way to convert byte[2560] to float[640]?
            for(int i = 0; i < VectorDimension; i++)
            {
                responseVector[i] = BitConverter.ToSingle(responseData, i * 4);
            }
            
            float[] resultVector = _pcaConversion.ApplyPCA(responseVector);

            return resultVector;
        }
    }
}
