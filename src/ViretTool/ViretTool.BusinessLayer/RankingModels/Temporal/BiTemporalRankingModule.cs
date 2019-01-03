using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRankingModule : IBiTemporalRankingModule
    {
        public IBiTemporalSimilarityModule BiTemporalSimilarityModule { get; }
        public IFilteringModule PrimaryFilteringModule { get; }
        public IFilteringModule SecondaryFilteringModule { get; }

        public TemporalQuery CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public BiTemporalRankingBuffer IntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer OutputRanking { get; private set; }

        public BiTemporalRankingBuffer ColorSketchIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer KeywordIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer SemanticExampleIntermediateRanking { get; private set; }

        
        public BiTemporalRankingModule(
            IBiTemporalSimilarityModule biTemporalSimilarityModule, 
            IFilteringModule primaryFilteringModule, IFilteringModule secondaryFilteringModule)
        {
            BiTemporalSimilarityModule = biTemporalSimilarityModule;
            PrimaryFilteringModule = primaryFilteringModule;
            SecondaryFilteringModule = secondaryFilteringModule;
        }


        public void ComputeRanking(TemporalQuery query, 
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

            if ((query == null && CachedQuery == null)
                || (query.Equals(CachedQuery) && !inputRanking.IsUpdated))
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
                if (IntermediateRanking == null)
                {
                    IntermediateRanking = BiTemporalRankingBuffer.Zeros(
                            "BiTemporalRankingModuleItermediate", InputRanking.Ranks.Length);
                }

                // transform query
                BiTemporalSimilarityQuery temporalSimilarityQuery = new BiTemporalSimilarityQuery(
                    new SimilarityQuery[] 
                    {
                        query.TemporalQueries[0].SimilarityQuery,
                        query.TemporalQueries[1].SimilarityQuery
                    });

                // compute ranking
                BiTemporalSimilarityModule.ComputeRanking(
                    temporalSimilarityQuery, InputRanking, IntermediateRanking,
                    PrimaryColorSketchRanking, PrimaryKeywordRanking, PrimarySemanticExampleRanking,
                    SecondaryColorSketchRanking, SecondaryKeywordRanking, SecondarySemanticExampleRanking);

                // compute filterings
                PrimaryFilteringModule.ComputeRanking(
                    query.TemporalQueries[0].FilteringQuery,
                    IntermediateRanking.PrimaryRankingBuffer, OutputRanking.PrimaryRankingBuffer,
                    PrimaryColorSketchRanking, PrimaryKeywordRanking, PrimarySemanticExampleRanking);

                SecondaryFilteringModule.ComputeRanking(
                    query.TemporalQueries[1].FilteringQuery,
                    IntermediateRanking.SecondaryRankingBuffer, OutputRanking.SecondaryRankingBuffer,
                    SecondaryColorSketchRanking, SecondaryKeywordRanking, SecondarySemanticExampleRanking);

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
