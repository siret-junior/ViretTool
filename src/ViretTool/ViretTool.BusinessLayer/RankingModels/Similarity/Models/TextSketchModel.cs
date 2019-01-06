using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public class TextSketchModel : BoolSketchModel, ITextSketchModel
    {
        public TextSketchModel(ITextSignatureDescriptorProvider textSignatureDescriptorProvider)
            : base(textSignatureDescriptorProvider)
        {
        }
    }
}
