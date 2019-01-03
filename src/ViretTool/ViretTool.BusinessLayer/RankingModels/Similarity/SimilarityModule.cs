using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Similarity
{
    public class SimilarityModule : ISimilarityModule
    {
        public IKeywordModel<KeywordQuery> KeywordModel { get; }
        public IColorSignatureModel<ColorSketchQuery> ColorSketchModel { get; }
        public ISemanticExampleModel<SemanticExampleQuery> SemanticExampleModel { get; }

        public IRankFusion RankFusion { get; }

        public SimilarityQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer KeywordIntermediateRanking { get; private set; }
        public RankingBuffer ColorIntermediateRanking { get; private set; }
        public RankingBuffer SemanticExampleIntermediateRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }

        
        public SimilarityModule(
            IKeywordModel<KeywordQuery> keywordModel,
            IColorSignatureModel<ColorSketchQuery> colorSketchModel,
            ISemanticExampleModel<SemanticExampleQuery> semanticExampleModel,
            IRankFusion rankFusion)
        {
            KeywordModel = keywordModel;
            ColorSketchModel = colorSketchModel;
            SemanticExampleModel = semanticExampleModel;
            RankFusion = rankFusion;
        }


        public void ComputeRanking(SimilarityQuery query, RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

            if ((query == null && CachedQuery == null) 
                || (query.Equals(CachedQuery) && !InputRanking.IsUpdated))
            {
                OutputRanking.IsUpdated = false;
                return;
            }
            OutputRanking.IsUpdated = true;

            if (query != null)
            {
                // create itermediate rankings if neccessary
                if (KeywordIntermediateRanking == null)
                {
                    KeywordIntermediateRanking 
                        = RankingBuffer.Zeros("KeywordModuleItermediate", InputRanking.Ranks.Length);
                }
                if (ColorIntermediateRanking == null)
                {
                    ColorIntermediateRanking 
                        = RankingBuffer.Zeros("ColorModuleItermediate", InputRanking.Ranks.Length);
                }
                if (SemanticExampleIntermediateRanking == null)
                {
                    SemanticExampleIntermediateRanking 
                        = RankingBuffer.Zeros("SemanticExampleModuleItermediate", InputRanking.Ranks.Length);
                }

                // compute partial rankings (if neccessary)
                KeywordModel.ComputeRanking(query.KeywordQuery, 
                    InputRanking, KeywordIntermediateRanking);
                ColorSketchModel.ComputeRanking(query.ColorSketchQuery, 
                    InputRanking, ColorIntermediateRanking);
                SemanticExampleModel.ComputeRanking(query.SemanticExampleQuery, 
                    InputRanking, SemanticExampleIntermediateRanking);

                // perform rank fusion (only with models used for sorting)
                List<RankingBuffer> buffersForFusion = new List<RankingBuffer>(3);
                if (query.KeywordQuery.UseForSorting)
                {
                    buffersForFusion.Add(KeywordIntermediateRanking);
                }
                if (query.ColorSketchQuery.UseForSorting)
                {
                    buffersForFusion.Add(ColorIntermediateRanking);
                }
                if (query.SemanticExampleQuery.UseForSorting)
                {
                    buffersForFusion.Add(SemanticExampleIntermediateRanking);
                }
                RankFusion.ComputeRanking(buffersForFusion.ToArray(), OutputRanking);
                
                // cache the query
                CachedQuery = query;
            }
            else
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
            }
        }
    }
}
