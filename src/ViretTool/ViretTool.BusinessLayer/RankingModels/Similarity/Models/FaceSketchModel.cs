using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public class FaceSketchModel : BoolSketchModel
    {
        public FaceSketchModel(IRankFusion rankFusion, bool[][] boolSignatures) 
            : base(rankFusion, boolSignatures)
        {
        }

        public FaceSketchModel()
            : base(null, null)
        {
        }
    }
}
