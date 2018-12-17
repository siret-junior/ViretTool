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
        public RankingModule(ISimilarityModule similarityModule, IFilteringModule filteringModule)
        {
            SimilarityModule = similarityModule;
            FilteringModule = filteringModule;
        }

        public int FrameCount { get; private set; }

        public ISimilarityModule SimilarityModule { get; }
        public IFilteringModule FilteringModule { get; }

        public Query CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer IntermediateRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }

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

                // compute partial rankings (if neccessary)
                SimilarityModule.ComputeRanking(query.SimilarityQuery, InputRanking, IntermediateRanking);
                FilteringModule.ComputeRanking(query.FilteringQuery, IntermediateRanking, OutputRanking);

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
