using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity
{
    public abstract class BiTemporalSimilarityModel<TSimilarityModel, TQuery>
        : IBiTemporalSimilarityModel<TSimilarityModel, TQuery> 
        where TSimilarityModel : ISimilarityModel<TQuery>
    {
        public TSimilarityModel PrimarySimilarityModel { get; private set; }
        public TSimilarityModel SecondarySimilarityModel { get; private set; }
        public IBiTemporalRankFusion RankFusion { get; private set; }

        public BiTemporalQuery<TQuery> CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer PrimaryIntermediateRanking { get; private set; }
        public RankingBuffer SecondaryIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer OutputRanking { get; private set; }

        
        public BiTemporalSimilarityModel(
            TSimilarityModel primarySimilarityModel,
            TSimilarityModel secondarySimilarityModel,
            IBiTemporalRankFusion rankFusion)
        {
            PrimarySimilarityModel = primarySimilarityModel;
            SecondarySimilarityModel = secondarySimilarityModel;
            RankFusion = rankFusion;
        }


        public virtual void ComputeRanking(BiTemporalQuery<TQuery> query, 
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking)
        {
            InputRanking = inputRanking;
            OutputRanking = outputRanking;

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
                // compute partial rankings (if neccessary)
                PrimarySimilarityModel.ComputeRanking(
                    query.PrimaryQuery, InputRanking, PrimaryIntermediateRanking);

                SecondarySimilarityModel.ComputeRanking(
                    query.SecondaryQuery, InputRanking, SecondaryIntermediateRanking);

                // temporal rank fusion
                RankFusion.ComputeRanking(PrimaryIntermediateRanking, SecondaryIntermediateRanking, OutputRanking);

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
