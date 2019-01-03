using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity.Models
{
    public class BiTemporalColorSignatureModel
        : BiTemporalSimilarityModel<IColorSignatureModel<ColorSketchQuery>, ColorSketchQuery>,
        IBiTemporalColorSignatureModel
    {
        public BiTemporalColorSignatureModel(
            IColorSignatureModel<ColorSketchQuery> primarySimilarityModel, 
            IColorSignatureModel<ColorSketchQuery> secondarySimilarityModel, 
            IBiTemporalRankFusion rankFusion) 
            : base(primarySimilarityModel, secondarySimilarityModel, rankFusion)
        {
        }
    }
}
