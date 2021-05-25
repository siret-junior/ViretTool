using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Ranking.W2VV
{
    public class W2vvTextToVectorRemote
    {
        public readonly string ServerUrl;

        public W2vvTextToVectorRemote(string serverUrl)
        {
            ServerUrl = serverUrl;
        }

        public /* async? */ float[] TextToVector(string[] query)
        {
            throw new NotImplementedException();
        }
    }
}
