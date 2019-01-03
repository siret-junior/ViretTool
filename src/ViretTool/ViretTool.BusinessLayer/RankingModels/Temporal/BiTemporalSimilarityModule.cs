using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Similarity.Models;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    class BiTemporalSimilarityModule : IBiTemporalSimilarityModule
    {
        public IBiTemporalKeywordModel BiTemporalKeywordModel { get; }
        public IBiTemporalColorSignatureModel BiTemporalColorSignatureModel { get; }
        public IBiTemporalSemanticExampleModel BiTemporalSemanticExampleModel { get; }
        public IRankFusion RankFusion { get; }

        public BiTemporalSimilarityQuery CachedQuery { get; private set; }
        public RankingBuffer InputRanking { get; private set; }
        public BiTemporalRankingBuffer KeywordIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer ColorSketchIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer SemanticExampleIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer OutputRanking { get; private set; }


        public BiTemporalSimilarityModule(
            IBiTemporalKeywordModel biTemporalKeywordModel, 
            IBiTemporalColorSignatureModel biTemporalColorSignatureModel, 
            IBiTemporalSemanticExampleModel biTemporalSemanticExampleModel, 
            IRankFusion rankFusion)
        {
            BiTemporalKeywordModel = biTemporalKeywordModel;
            BiTemporalColorSignatureModel = biTemporalColorSignatureModel;
            BiTemporalSemanticExampleModel = biTemporalSemanticExampleModel;
            RankFusion = rankFusion;
        }


        public void ComputeRanking(BiTemporalSimilarityQuery query, 
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking,
            BiTemporalRankingBuffer colorSketchIntermediateRanking,
            BiTemporalRankingBuffer keywordIntermediateRanking,
            BiTemporalRankingBuffer semanticExampleIntermediateRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;
            ColorSketchIntermediateRanking = colorSketchIntermediateRanking;
            KeywordIntermediateRanking = keywordIntermediateRanking;
            SemanticExampleIntermediateRanking = semanticExampleIntermediateRanking;

            if ((query == null && CachedQuery == null)
                || (query.Equals(CachedQuery) && !InputRanking.IsUpdated))
            {
                OutputRanking.PrimaryRankingBuffer.IsUpdated = false;
                OutputRanking.SecondaryRankingBuffer.IsUpdated = false;
                return;
            }
            OutputRanking.PrimaryRankingBuffer.IsUpdated = true;
            OutputRanking.SecondaryRankingBuffer.IsUpdated = true;


            if (query != null)
            {
                // create itermediate rankings if neccessary
                if (KeywordIntermediateRanking == null)
                {
                    KeywordIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                        "BiTemporalKeywordModuleItermediate", InputRanking.Ranks.Length);
                }
                if (ColorSketchIntermediateRanking == null)
                {
                    ColorSketchIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                        "BiTemporalColorSketchModuleItermediate", InputRanking.Ranks.Length);
                }
                if (SemanticExampleIntermediateRanking == null)
                {
                    SemanticExampleIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                        "BiTemporalSemanticExampleModuleItermediate", InputRanking.Ranks.Length);
                }


                // compute partial rankings (if neccessary)
                SimilarityQuery primaryQuery = query.PrimaryQuery;
                SimilarityQuery secondaryQuery = query.SecondaryQuery;

                BiTemporalKeywordModel.ComputeRanking(
                    new BiTemporalQuery<KeywordQuery>(primaryQuery.KeywordQuery, secondaryQuery.KeywordQuery), 
                    InputRanking, KeywordIntermediateRanking);

                BiTemporalColorSignatureModel.ComputeRanking(
                    primaryQuery.ColorSketchQuery, secondaryQuery.ColorSketchQuery,
                    InputRanking, ColorSketchIntermediateRanking);
                BiTemporalSemanticExampleModel.ComputeRanking(
                    primaryQuery.SemanticExampleQuery, secondaryQuery.SemanticExampleQuery,
                    InputRanking, SemanticExampleIntermediateRanking);

                // perform rank fusion (only with models used for sorting)
                // primary
                List<RankingBuffer> primaryBuffersForFusion = new List<RankingBuffer>(3);
                if (primaryQuery.KeywordQuery.UseForSorting)
                {
                    primaryBuffersForFusion.Add(KeywordIntermediateRanking.PrimaryRankingBuffer);
                }
                if (primaryQuery.ColorSketchQuery.UseForSorting)
                {
                    primaryBuffersForFusion.Add(ColorIntermediateRanking.PrimaryRankingBuffer);
                }
                if (primaryQuery.SemanticExampleQuery.UseForSorting)
                {
                    primaryBuffersForFusion.Add(SemanticExampleIntermediateRanking.PrimaryRankingBuffer);
                }
                RankFusion.ComputeRanking(primaryBuffersForFusion.ToArray(), OutputRanking.PrimaryRankingBuffer);
                
                // secondary
                List<RankingBuffer> secondaryBuffersForFusion = new List<RankingBuffer>(3);
                if (secondaryQuery.KeywordQuery.UseForSorting)
                {
                    secondaryBuffersForFusion.Add(KeywordIntermediateRanking.SecondaryRankingBuffer);
                }
                if (secondaryQuery.ColorSketchQuery.UseForSorting)
                {
                    secondaryBuffersForFusion.Add(ColorIntermediateRanking.SecondaryRankingBuffer);
                }
                if (secondaryQuery.SemanticExampleQuery.UseForSorting)
                {
                    secondaryBuffersForFusion.Add(SemanticExampleIntermediateRanking.SecondaryRankingBuffer);
                }
                RankFusion.ComputeRanking(secondaryBuffersForFusion.ToArray(), OutputRanking.SecondaryRankingBuffer);

                // cache the query
                CachedQuery = query;
            }
            else
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.PrimaryRankingBuffer.Ranks, InputRanking.Ranks.Length);
                Array.Copy(InputRanking.Ranks, OutputRanking.SecondaryRankingBuffer.Ranks, InputRanking.Ranks.Length);
            }
        }
    }
}
