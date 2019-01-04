//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ViretTool.BusinessLayer.RankingModels.Queries;
//using ViretTool.BusinessLayer.RankingModels.Similarity.Models;

//namespace ViretTool.BusinessLayer.RankingModels.Similarity
//{
//    public interface IFilteredSimilarityModel<TQuery, TSimilarityModel>
//        where TQuery : IQuery
//        where TSimilarityModel : ISimilarityModel<TQuery>
//    {
//        TSimilarityModel SimilarityModel { get; }

//        FilteredRankingQuery<TQuery> CachedQuery { get; }

//        RankingBuffer InputRanking { get; }
//        RankingBuffer IntermediateRanking { get; }
//        RankingBuffer OutputRanking { get; }

//        void ComputeRanking(FilteredRankingQuery<TQuery> query, 
//            RankingBuffer inputRanking, RankingBuffer outputRanking);
//    }
//}
