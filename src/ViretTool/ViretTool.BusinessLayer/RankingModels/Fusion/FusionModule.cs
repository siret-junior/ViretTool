using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Fusion
{
    public class FusionModule : IFusionModule
    {
        public IRankFilteringModule KeywordRankFilteringModule { get; }
        public IRankFilteringModule ColorSketchRankFilteringModule { get; }
        public IRankFilteringModule FaceSketchRankFilteringModule { get; }
        public IRankFilteringModule TextSketchRankFilteringModule { get; }
        public IRankFilteringModule SemanticExampleRankFilteringModule { get; }
        
        public FusionQuery CachedQuery { get; private set; }
        
        public RankingBuffer KeywordRanking { get; private set; }
        public RankingBuffer ColorSketchRanking { get; private set; }
        public RankingBuffer FaceSketchRanking { get; private set; }
        public RankingBuffer TextSketchRanking { get; private set; }
        public RankingBuffer SemanticExampleRanking { get; private set; }

        public RankingBuffer KeywordIntermediateRanking { get; private set; }
        public RankingBuffer ColorSketchIntermediateRanking { get; private set; }
        public RankingBuffer FaceSketchIntermediateRanking { get; private set; }
        public RankingBuffer TextSketchIntermediateRanking { get; private set; }
        public RankingBuffer SemanticExampleIntermediateRanking { get; private set; }
        
        public RankingBuffer OutputRanking { get; private set; }


        public FusionModule(
            IRankFilteringModule keywordRankFilteringModule, 
            IRankFilteringModule colorSketchRankFilteringModule, 
            IRankFilteringModule faceSketchRankFilteringModule, 
            IRankFilteringModule textSketchRankFilteringModule, 
            IRankFilteringModule semanticExampleRankFilteringModule)
        {
            KeywordRankFilteringModule = keywordRankFilteringModule;
            ColorSketchRankFilteringModule = colorSketchRankFilteringModule;
            FaceSketchRankFilteringModule = faceSketchRankFilteringModule;
            TextSketchRankFilteringModule = textSketchRankFilteringModule;
            SemanticExampleRankFilteringModule = semanticExampleRankFilteringModule;
        }


        public void ComputeRanking(
            FusionQuery query, 
            RankingBuffer keywordRanking, 
            RankingBuffer colorSketchRanking, 
            RankingBuffer faceSketchRanking, 
            RankingBuffer textSketchRanking, 
            RankingBuffer semanticExampleRanking, 
            RankingBuffer outputRanking)
        {
            KeywordRanking = keywordRanking;
            ColorSketchRanking = colorSketchRanking;
            FaceSketchRanking = faceSketchRanking;
            TextSketchRanking = textSketchRanking;
            SemanticExampleRanking = semanticExampleRanking;
            OutputRanking = outputRanking;
            InitializeIntermediateBuffers();

            if (!HasQueryOrInputChanged(query))
            {
                // nothing changed, OutputRanking contains cached data from previous computation
                OutputRanking.IsUpdated = false;
                return;
            }
            else
            {
                CachedQuery = query;
                OutputRanking.IsUpdated = true;
            }

            // rank filtering
            KeywordRankFilteringModule.ComputeFiltering(
                query.KeywordFilteringQuery, KeywordRanking, KeywordIntermediateRanking);
            ColorSketchRankFilteringModule.ComputeFiltering(
               query.ColorSketchFilteringQuery, ColorSketchRanking, ColorSketchIntermediateRanking);
            //FaceSketchRankFilteringModule.ComputeFiltering(
            //   query.FaceSketchFilteringQuery, FaceSketchRanking, FaceSketchIntermediateRanking);
            //TextSketchRankFilteringModule.ComputeFiltering(
            //   query.TextSketchFilteringQuery, TextSketchRanking, TextSketchIntermediateRanking);
            SemanticExampleRankFilteringModule.ComputeFiltering(
               query.SemanticExampleFilteringQuery, SemanticExampleRanking, SemanticExampleIntermediateRanking);

            // prepare rank arrays
            float[][] filteringRanks = new float[][]
            {
                KeywordIntermediateRanking.Ranks,
                ColorSketchIntermediateRanking.Ranks,
                //FaceSketchIntermediateRanking.Ranks,
                //TextSketchIntermediateRanking.Ranks,
                FaceSketchRanking.Ranks,
                TextSketchRanking.Ranks,
                SemanticExampleIntermediateRanking.Ranks
            };

            // prepare sort array
            float[] sortingRanks;
            switch (query.SortingSimilarityModel)
            {
                case FusionQuery.SimilarityModels.Keyword:
                    sortingRanks = KeywordIntermediateRanking.Ranks;
                    break;
                case FusionQuery.SimilarityModels.ColorSketch:
                    sortingRanks = ColorSketchIntermediateRanking.Ranks;
                    break;
                //case FusionQuery.SimilarityModels.FaceSketch:
                //    sortingRanks = FaceSketchIntermediateRanking.Ranks;
                //    break;
                //case FusionQuery.SimilarityModels.TextSketch:
                //    sortingRanks = TextSketchIntermediateRanking.Ranks;
                //    break;
                case FusionQuery.SimilarityModels.SemanticExample:
                    sortingRanks = SemanticExampleIntermediateRanking.Ranks;
                    break;
                default:
                    // no model for sorting is selected, just filter results
                    sortingRanks = null;
                    break;
                    //throw new NotImplementedException(
                    //    "Model for rank sorting " +
                    //    Enum.GetName(typeof(FilterState), query.SortingSimilarityModel) +
                    //    " not expected.");
            }

            // filter/rank aggregation (float.MinValue means that the rank it is filtered)
            if (sortingRanks != null)
            {
                ComputeFusion(filteringRanks, sortingRanks, OutputRanking);
            }
            else
            {
                ComputeFilteringOnly(filteringRanks, OutputRanking);
            }
        }

        private void InitializeIntermediateBuffers()
        {
            // create itermediate rankings if neccessary
            if (KeywordIntermediateRanking == null
                || KeywordIntermediateRanking.Ranks.Length != KeywordRanking.Ranks.Length)
            {
                KeywordIntermediateRanking = RankingBuffer.Zeros(
                    "KeywordIntermediateRanking", KeywordRanking.Ranks.Length);
                ColorSketchIntermediateRanking = RankingBuffer.Zeros(
                    "ColorSketchIntermediateRanking", KeywordRanking.Ranks.Length);
                FaceSketchIntermediateRanking = RankingBuffer.Zeros(
                    "FaceSketchIntermediateRanking", KeywordRanking.Ranks.Length);
                TextSketchIntermediateRanking = RankingBuffer.Zeros(
                    "TextSketchIntermediateRanking", KeywordRanking.Ranks.Length);
                SemanticExampleIntermediateRanking = RankingBuffer.Zeros(
                    "SemanticExampleIntermediateRanking", KeywordRanking.Ranks.Length);
            }
        }

        private bool HasQueryOrInputChanged(FusionQuery query)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || KeywordRanking.IsUpdated
                || ColorSketchRanking.IsUpdated
                || FaceSketchRanking.IsUpdated
                || TextSketchRanking.IsUpdated
                || SemanticExampleRanking.IsUpdated;
        }

        private void ComputeFusion(float[][] filteringRanks, float[] sortingRanks, RankingBuffer outputRanking)
        {
            Parallel.For(0, outputRanking.Ranks.Length, itemId =>
            {
                // return sorting ranks, but only if they are not filtered
                for (int iRanking = 0; iRanking < filteringRanks.Length; iRanking++)
                {
                    if (filteringRanks[iRanking][itemId] == float.MinValue)
                    {
                        outputRanking.Ranks[itemId] = float.MinValue;
                        return;
                    }
                }
                outputRanking.Ranks[itemId] = sortingRanks[itemId];
            });
        }

        private void ComputeFilteringOnly(float[][] filteringRanks, RankingBuffer outputRanking)
        {
            Parallel.For(0, outputRanking.Ranks.Length, itemId =>
            {
                // return sorting ranks, but only if they are not filtered
                for (int iRanking = 0; iRanking < filteringRanks.Length; iRanking++)
                {
                    if (filteringRanks[iRanking][itemId] == float.MinValue)
                    {
                        outputRanking.Ranks[itemId] = float.MinValue;
                        return;
                    }
                }
                outputRanking.Ranks[itemId] = 0;
            });
        }
    }
}
