using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Similarity
{
    public interface IBiTemporalSimilarityModel<TSimilarityModel, TQuery>
    {
        TSimilarityModel PrimarySimilarityModel { get; }
        TSimilarityModel SecondarySimilarityModel { get; }
        IBiTemporalRankFusion RankFusion { get; }

        BiTemporalQuery<TQuery> CachedQuery { get; }

        RankingBuffer InputRanking { get; }
        RankingBuffer PrimaryIntermediateRanking { get; }
        RankingBuffer SecondaryIntermediateRanking { get; }
        BiTemporalRankingBuffer OutputRanking { get; }
        
        void ComputeRanking(BiTemporalQuery<TQuery> query, 
            RankingBuffer inputRanking, BiTemporalRankingBuffer outputRanking);
        
    }
}
