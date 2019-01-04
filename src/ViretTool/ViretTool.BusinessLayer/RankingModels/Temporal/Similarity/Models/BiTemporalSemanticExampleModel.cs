//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.BusinessLayer.RankingModels.Queries;
//using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

//namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity.Models
//{
//    public class BiTemporalSemanticExampleModel
//        : BiTemporalSimilarityModel<ISemanticExampleModel<SemanticExampleQuery>, SemanticExampleQuery>,
//        IBiTemporalSemanticExampleModel
//    {
//        public BiTemporalSemanticExampleModel(
//            ISemanticExampleModel<SemanticExampleQuery> primarySimilarityModel, 
//            ISemanticExampleModel<SemanticExampleQuery> secondarySimilarityModel, 
//            IBiTemporalRankFusion rankFusion) 
//            : base(primarySimilarityModel, secondarySimilarityModel, rankFusion)
//        {
//        }
//    }
//}
