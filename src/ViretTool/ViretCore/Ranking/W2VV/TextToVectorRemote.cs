using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.W2VV
{
    public class TextToVectorRemote
    {
        public readonly string ServerUrl;

        public TextToVectorRemote(string inputDirectory, string subdirectory, string serverUrl)
        {
            ServerUrl = serverUrl;
        }

        public /* async? */ float[] TextToVector(string[] query)
        {
            throw new NotImplementedException();
        }
    }
}
