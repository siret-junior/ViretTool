using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Fusion;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRankingModule : IBiTemporalRankingModule
    {
        public IBiTemporalSimilarityModule BiTemporalSimilarityModule { get; }
        public IFusionModule FormerFusionModule { get; }
        public IFusionModule LatterFusionModule { get; }
        public IFilteringModule FormerFilteringModule { get; }
        public IFilteringModule LatterFilteringModule { get; }

        public BiTemporalQuery CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }

        public BiTemporalRankingBuffer KeywordIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer ColorSketchIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer FaceSketchIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer TextSketchIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer SemanticExampleIntermediateRanking { get; private set; }

        public BiTemporalRankingBuffer IntermediateFusionRanking { get; private set; }

        public BiTemporalRankingBuffer OutputRanking { get; private set; }

        

        public BiTemporalRankingModule(
            IBiTemporalSimilarityModule biTemporalSimilarityModule,
            IFusionModule formerFusionModule,
            IFusionModule latterFusionModule,
            IFilteringModule formerFilteringModule, 
            IFilteringModule latterFilteringModule)
        {
            BiTemporalSimilarityModule = biTemporalSimilarityModule;
            FormerFusionModule = formerFusionModule;
            LatterFusionModule = latterFusionModule;
            FormerFilteringModule = formerFilteringModule;
            LatterFilteringModule = latterFilteringModule;
        }


        public void ComputeRanking(BiTemporalQuery query, 
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;
            InitializeIntermediateBuffers();

            if (!HasQueryOrInputChanged(query, inputRanking))
            {
                // nothing changed, OutputRanking contains cached data from previous computation
                OutputRanking.FormerRankingBuffer.IsUpdated = false;
                OutputRanking.LatterRankingBuffer.IsUpdated = false;
                return;
            }
            else
            {
                CachedQuery = query;
                OutputRanking.FormerRankingBuffer.IsUpdated = true;
                OutputRanking.LatterRankingBuffer.IsUpdated = true;
            }

            if (IsQueryEmpty(query))
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.FormerRankingBuffer.Ranks, InputRanking.Ranks.Length);
                Array.Copy(InputRanking.Ranks, OutputRanking.LatterRankingBuffer.Ranks, InputRanking.Ranks.Length);
                return;
            }
            

            // BiTemporal similarity model ranks only, separately
            BiTemporalSimilarityModule.ComputeRanking(
                query.BiTemporalSimilarityQuery, 
                InputRanking,
                KeywordIntermediateRanking,
                ColorSketchIntermediateRanking,
                FaceSketchIntermediateRanking,
                TextSketchIntermediateRanking,
                SemanticExampleIntermediateRanking);

            // similarity model ranks fusion (side by side temporal)
            FormerFusionModule.ComputeRanking(
                query.FormerFusionQuery,
                KeywordIntermediateRanking.FormerRankingBuffer,
                ColorSketchIntermediateRanking.FormerRankingBuffer,
                FaceSketchIntermediateRanking.FormerRankingBuffer,
                TextSketchIntermediateRanking.FormerRankingBuffer,
                SemanticExampleIntermediateRanking.FormerRankingBuffer,
                IntermediateFusionRanking.FormerRankingBuffer);
            LatterFusionModule.ComputeRanking(
                query.LatterFusionQuery,
                KeywordIntermediateRanking.LatterRankingBuffer,
                ColorSketchIntermediateRanking.LatterRankingBuffer,
                FaceSketchIntermediateRanking.LatterRankingBuffer,
                TextSketchIntermediateRanking.LatterRankingBuffer,
                SemanticExampleIntermediateRanking.LatterRankingBuffer,
                IntermediateFusionRanking.LatterRankingBuffer);

            // assign temporal pairs hotfix
            AssignTemporalPairs(
                query.FormerFusionQuery.SortingSimilarityModel,
                KeywordIntermediateRanking.FormerTemporalPairs,
                ColorSketchIntermediateRanking.FormerTemporalPairs,
                FaceSketchIntermediateRanking.FormerTemporalPairs,
                TextSketchIntermediateRanking.FormerTemporalPairs,
                SemanticExampleIntermediateRanking.FormerTemporalPairs,
                IntermediateFusionRanking.FormerTemporalPairs);
            AssignTemporalPairs(
                query.LatterFusionQuery.SortingSimilarityModel,
                KeywordIntermediateRanking.LatterTemporalPairs,
                ColorSketchIntermediateRanking.LatterTemporalPairs,
                FaceSketchIntermediateRanking.LatterTemporalPairs,
                TextSketchIntermediateRanking.LatterTemporalPairs,
                SemanticExampleIntermediateRanking.LatterTemporalPairs,
                IntermediateFusionRanking.LatterTemporalPairs);

            // fusion ranks fitering (side by side temporal)
            FormerFilteringModule.ComputeRanking(
                query.FormerFilteringQuery,
                IntermediateFusionRanking.FormerRankingBuffer, 
                OutputRanking.FormerRankingBuffer);
            // TODO: just reassign pointer for FormerTemporalPairs
            Array.Copy(
                IntermediateFusionRanking.FormerTemporalPairs, 
                OutputRanking.FormerTemporalPairs,
                IntermediateFusionRanking.FormerTemporalPairs.Length);
            LatterFilteringModule.ComputeRanking(
                query.LatterFilteringQuery,
                IntermediateFusionRanking.LatterRankingBuffer, 
                OutputRanking.LatterRankingBuffer);
            Array.Copy(
                IntermediateFusionRanking.LatterTemporalPairs,
                OutputRanking.LatterTemporalPairs,
                IntermediateFusionRanking.LatterTemporalPairs.Length);
        }

        private void AssignTemporalPairs(
            FusionQuery.SimilarityModels sortingSimilarityModel, 
            int[] keywordTemporalPairs, 
            int[] colorSketchTemporalPairs, 
            int[] faceSketchTemporalPairs, 
            int[] textSketchTemporalPairs, 
            int[] semanticExampleTemporalPairs, 
            int[] outputTemporalPairs)
        {
            int[] temporalPairs;
            switch (sortingSimilarityModel)
            {
                case FusionQuery.SimilarityModels.Keyword:
                    temporalPairs = keywordTemporalPairs;
                    break;
                case FusionQuery.SimilarityModels.ColorSketch:
                    temporalPairs = colorSketchTemporalPairs;
                    break;
                


                case FusionQuery.SimilarityModels.SemanticExample:
                    temporalPairs = semanticExampleTemporalPairs;
                    break;
                default:
                    // no model for sorting is selected, just filter results
                    temporalPairs = null;
                    break;
            }
            if (temporalPairs != null)
            {
                Array.Copy(temporalPairs, outputTemporalPairs, temporalPairs.Length);
            }
            else
            {
                outputTemporalPairs = null;
            }
        }

        private bool HasQueryOrInputChanged(BiTemporalQuery query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(BiTemporalQuery query)
        {
            return query == null;
        }
        

        private void InitializeIntermediateBuffers()
        {
            // create itermediate rankings if neccessary
            if (KeywordIntermediateRanking == null
                || KeywordIntermediateRanking.FormerRankingBuffer.Ranks.Length != InputRanking.Ranks.Length)
            {
                KeywordIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                    "KeywordIntermediateRanking", InputRanking.Ranks.Length);
                ColorSketchIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                    "ColorSketchIntermediateRanking", InputRanking.Ranks.Length);
                FaceSketchIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                    "FaceSketchIntermediateRanking", InputRanking.Ranks.Length);
                TextSketchIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                    "TextSketchIntermediateRanking", InputRanking.Ranks.Length);
                SemanticExampleIntermediateRanking = BiTemporalRankingBuffer.Zeros(
                    "SemanticExampleIntermediateRanking", InputRanking.Ranks.Length);

                IntermediateFusionRanking = BiTemporalRankingBuffer.Zeros(
                    "IntermediateFusionRanking", InputRanking.Ranks.Length);
            }
        }


    }
}
