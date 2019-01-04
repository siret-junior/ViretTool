//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.BusinessLayer.RankingModels.Queries;
//using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
//using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

//namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity.Models
//{
//    public class BiTemporalColorSignatureModel
//        : BiTemporalSimilarityModel<IColorSketchModel<ColorSketchQuery>, ColorSketchQuery>,
//        IBiTemporalColorSignatureModel
//    {
//        public BiTemporalColorSignatureModel(
//            IColorSketchModel<ColorSketchQuery> primarySimilarityModel, 
//            IColorSketchModel<ColorSketchQuery> secondarySimilarityModel, 
//            IBiTemporalRankFusion rankFusion) 
//            : base(primarySimilarityModel, secondarySimilarityModel, rankFusion)
//        {
//        }
//    }
//}
