using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class RankingModule : IRankingModule
    {
        public ISimilarityModule SimilarityModule { get; }
        public IFilteringModule FilteringModule { get; }

        public Query CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer IntermediateRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }


        public RankingModule(ISimilarityModule similarityModule, IFilteringModule filteringModule)
        {
            SimilarityModule = similarityModule;
            FilteringModule = filteringModule;
        }


        public void ComputeRanking(Query query, RankingBuffer inputRanking, RankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

            if ((query == null && CachedQuery == null) 
                || (query.Equals(CachedQuery) && !inputRanking.IsUpdated))
            {
                OutputRanking.IsUpdated = false;
                return;
            }
            OutputRanking.IsUpdated = true;

            if (query != null)
            {
                // create itermediate rankings if neccessary
                if (IntermediateRanking == null)
                {
                    IntermediateRanking
                        = RankingBuffer.Zeros("SimilarityModuleItermediate", InputRanking.Ranks.Length);
                }

                SimilarityModule.ComputeRanking(query.SimilarityQuery, InputRanking, IntermediateRanking);

                // compute filtering
                RankingBuffer colorSketchRanking = null;
                if (query.SimilarityQuery.ColorSketchQuery.UseForFiltering)
                {
                    colorSketchRanking = SimilarityModule.ColorSketchModel.OutputRanking;
                }
                RankingBuffer keywordRanking = null;
                if (query.SimilarityQuery.KeywordQuery.UseForFiltering)
                {
                    keywordRanking = SimilarityModule.KeywordModel.OutputRanking;
                }
                RankingBuffer semanticExampleRanking = null;
                if (query.SimilarityQuery.SemanticExampleQuery.UseForFiltering)
                {
                    semanticExampleRanking = SimilarityModule.SemanticExampleModel.OutputRanking;
                }
                FilteringModule.ComputeRanking(query.FilteringQuery, IntermediateRanking, OutputRanking,
                    colorSketchRanking, keywordRanking, semanticExampleRanking);

                // cache the query
                CachedQuery = query;
            }
            else
            {
                // no query, output is the same as input
                Array.Copy(InputRanking.Ranks, OutputRanking.Ranks, InputRanking.Ranks.Length);
                
                // TODO: this is unsafe, we might rewrite important data
                //OutputRanking.Ranks = InputRanking.Ranks;
            }
        }
    }
}
