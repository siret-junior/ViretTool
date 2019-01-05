using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public class TextSketchModel : BoolSketchModel
    {
        public TextSketchModel(/*IRankFusion rankFusion, */bool[][] boolSignatures) 
            : base(/*rankFusion, */boolSignatures)
        {
        }


        public TextSketchModel()
            : base(/*null, */null)
        {
        }
    }
}
