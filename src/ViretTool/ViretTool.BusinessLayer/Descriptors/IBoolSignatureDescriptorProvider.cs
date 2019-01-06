using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public interface IBoolSignatureDescriptorProvider
    {
        byte[] DatasetHeader { get; }

        int DescriptorCount { get; }
        int DescriptorLength { get; }

        bool[][] Descriptors { get; }
        int SignatureWidth { get; }
        int SignatureHeight { get; }
    }
}
