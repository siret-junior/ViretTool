using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public class FaceSketchModel : BoolSketchModel, IFaceSketchModel
    {
        public FaceSketchModel(IFaceSignatureDescriptorProvider faceSignatureDescriptorProvider) 
            : base(faceSignatureDescriptorProvider)
        {
        }
    }
}
