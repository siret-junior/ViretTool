using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity
{
    public class BiTemporalSimilarityModel<TQuery, TSimilarityModel, TRankFusion>
        : IBiTemporalSimilarityModel<TQuery, TSimilarityModel, TRankFusion>
        where TQuery : IQuery
        where TSimilarityModel : ISimilarityModel<TQuery>
        where TRankFusion : IBiTemporalRankFusion
    {
        public TSimilarityModel FormerSimilarityModel { get; }
        public TSimilarityModel LatterSimilarityModel { get; }
        public IBiTemporalRankFusion BiTemporalRankFusion { get; private set; }

        public BiTemporalModelQuery<TQuery> CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer FormerIntermediateRanking { get; private set; }
        public RankingBuffer LatterIntermediateRanking { get; private set; }
        public BiTemporalRankingBuffer OutputRanking { get; private set; }


        public BiTemporalSimilarityModel(
            TSimilarityModel formerSimilarityModel, 
            TSimilarityModel latterSimilarityModel,
            IBiTemporalRankFusion biTemporalRankFusion)
        {
            FormerSimilarityModel = formerSimilarityModel;
            LatterSimilarityModel = latterSimilarityModel;
            BiTemporalRankFusion = biTemporalRankFusion;
        }


        public virtual void ComputeRanking(BiTemporalModelQuery<TQuery> query, 
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

            
            FormerSimilarityModel.ComputeRanking(query.FormerQuery, inputRanking, FormerIntermediateRanking);
            LatterSimilarityModel.ComputeRanking(query.LatterQuery, inputRanking, LatterIntermediateRanking);
            
            BiTemporalRankFusion.ComputeRanking(FormerIntermediateRanking, LatterIntermediateRanking, OutputRanking);
        }

        private void InitializeIntermediateBuffers()
        {
            // create itermediate rankings if neccessary
            if (FormerIntermediateRanking == null
                || FormerIntermediateRanking.Ranks.Length != InputRanking.Ranks.Length)
            {
                FormerIntermediateRanking = RankingBuffer.Zeros(
                    "FormerIntermediateRanking", InputRanking.Ranks.Length);
            }
            if (LatterIntermediateRanking == null
                || LatterIntermediateRanking.Ranks.Length != InputRanking.Ranks.Length)
            {
                LatterIntermediateRanking = RankingBuffer.Zeros(
                    "LatterIntermediateRanking", InputRanking.Ranks.Length);
            }
        }

        private bool HasQueryOrInputChanged(BiTemporalModelQuery<TQuery> query, RankingBuffer inputRanking)
        {
            return (query == null && CachedQuery != null)
                || (CachedQuery == null && query != null)
                || !query.Equals(CachedQuery)
                || inputRanking.IsUpdated;
        }

        private bool IsQueryEmpty(BiTemporalModelQuery<TQuery> query)
        {
            return query == null;
        }
    }
}
