using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public interface IW2VVQueryToVectorProvider
    {
        float[] TextToVector(string[] query, bool applyPCA = true);
    }
}
