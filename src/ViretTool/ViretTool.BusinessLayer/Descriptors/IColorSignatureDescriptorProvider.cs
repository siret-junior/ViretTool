using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public interface IColorSignatureDescriptorProvider : IDescriptorProvider<byte[]>
    {
        int SignatureWidth { get; }
        int SignatureHeight { get; }
    }
}
