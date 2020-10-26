using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors.Models;

namespace ViretTool.BusinessLayer.Descriptors
{
    public interface ILifelogDescriptorProvider : IDescriptorProvider<LifelogFrameMetadata>
    {
        string GetFilenameForFrame(int videoId, int frameNumber);
    }
}
