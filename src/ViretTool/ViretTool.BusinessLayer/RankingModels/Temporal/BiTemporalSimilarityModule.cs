using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalSimilarityModule : IBiTemporalSimilarityModule
    {
        public IBiTemporalSimilarityModel<KeywordQuery, IKeywordModel> KeywordModel { get; }
        public IBiTemporalSimilarityModel<ColorSketchQuery, IColorSketchModel> ColorSketchModel { get; }
        public IBiTemporalSimilarityModel<ColorSketchQuery, IFaceSketchModel> FaceSketchModel { get; }
        public IBiTemporalSimilarityModel<ColorSketchQuery, ITextSketchModel> TextSketchModel { get; }
        public IBiTemporalSimilarityModel<SemanticExampleQuery, ISemanticExampleModel> SemanticExampleModel { get; }
        
        public BiTemporalSimilarityQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; private set; }
        public BiTemporalRankingBuffer KeywordOutputRanking { get; private set; }
        public BiTemporalRankingBuffer ColorSketchOutputRanking { get; private set; }
        public BiTemporalRankingBuffer FaceSketchOutputRanking { get; private set; }
        public BiTemporalRankingBuffer TextSketchOutputRanking { get; private set; }
        public BiTemporalRankingBuffer SemanticExampleOutputRanking { get; private set; }


        public BiTemporalSimilarityModule(
            IBiTemporalSimilarityModel<KeywordQuery, IKeywordModel> keywordModel, 
            IBiTemporalSimilarityModel<ColorSketchQuery, IColorSketchModel> colorSketchModel, 
            IBiTemporalSimilarityModel<ColorSketchQuery, IFaceSketchModel> faceSketchModel, 
            IBiTemporalSimilarityModel<ColorSketchQuery, ITextSketchModel> textSketchModel, 
            IBiTemporalSimilarityModel<SemanticExampleQuery, ISemanticExampleModel> semanticExampleModel)
        {
            KeywordModel = keywordModel;
            ColorSketchModel = colorSketchModel;
            FaceSketchModel = faceSketchModel;
            TextSketchModel = textSketchModel;
            SemanticExampleModel = semanticExampleModel;
        }


        public void ComputeRanking(
            BiTemporalSimilarityQuery query, 
            RankingBuffer inputRanking,
            BiTemporalRankingBuffer keywordOutputRanking,
            BiTemporalRankingBuffer colorSketchOutputRanking,
            BiTemporalRankingBuffer faceSketchOutputRanking,
            BiTemporalRankingBuffer textSketchOutputRanking,
            BiTemporalRankingBuffer semanticExampleOutputRanking)
        {
            InputRanking = inputRanking;
            KeywordOutputRanking = keywordOutputRanking;
            ColorSketchOutputRanking = colorSketchOutputRanking;
            FaceSketchOutputRanking = faceSketchOutputRanking;
            TextSketchOutputRanking = textSketchOutputRanking;
            SemanticExampleOutputRanking = semanticExampleOutputRanking;
            
            // TODO: here caching is disabled
            //InitializeIntermediateBuffers();

            //if (!HasQueryOrInputChanged(query, inputRanking))
            //{
            //    // nothing changed, OutputRanking contains cached data from previous computation
            //    OutputRanking.FormerRankingBuffer.IsUpdated = false;
            //    OutputRanking.LatterRankingBuffer.IsUpdated = false;
            //    return;
            //}
            //else
            //{
            //    CachedQuery = query;
            //    OutputRanking.FormerRankingBuffer.IsUpdated = true;
            //    OutputRanking.LatterRankingBuffer.IsUpdated = true;
            //}

            if (IsQueryEmpty(query))
            {
                throw new NotImplementedException("BiTemporalSimilarityQuery is not expected to be empty here.");
            }
            

            KeywordModel.ComputeRanking(query.KeywordQuery, InputRanking, KeywordOutputRanking);
            ColorSketchModel.ComputeRanking(query.ColorSketchQuery, InputRanking, ColorSketchOutputRanking);
            //FaceSketchModel.ComputeRanking(query.FaceSketchQuery, InputRanking, FaceSketchOutputRanking);
            //TextSketchModel.ComputeRanking(query.TextSketchQuery, InputRanking, TextSketchOutputRanking);
            SemanticExampleModel.ComputeRanking(query.SemanticExampleQuery, InputRanking, SemanticExampleOutputRanking);
        }

        private bool IsQueryEmpty(BiTemporalSimilarityQuery query)
        {
            return query == null;
        }
        
    }
}
