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
        public IKeywordModel<KeywordQuery> KeywordModel { get; set; }
        public IColorSketchModel<ColorSketchQuery> ColorSketchModel { get; set; }
        public ISemanticExampleModel<SemanticExampleQuery> SemanticExampleModel { get; set; }

        public IRankFusion RankFusion { get; set; }

        public SimilarityQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; set; }
        public RankingBuffer KeywordIntermediateRanking { get; set; }
        public RankingBuffer ColorIntermediateRanking { get; set; }
        public RankingBuffer SemanticExampleIntermediateRanking { get; set; }
        public RankingBuffer OutputRanking { get; set; }

        public SimilarityModule()
        {
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

                // perform rank fusion
                RankFusion.ComputeRanking(
                    new RankingBuffer[] 
                    {
                        KeywordIntermediateRanking,
                        ColorIntermediateRanking,
                        SemanticExampleIntermediateRanking
                    },
                    OutputRanking);
                
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
