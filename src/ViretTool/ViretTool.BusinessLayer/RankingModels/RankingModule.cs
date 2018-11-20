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
        public ISimilarityModule SimilarityModule { get; private set; }
        public IFilteringModule FilteringModule { get; private set; }

        public Query CachedQuery { get; private set; }
        public Ranking InputRanking { get; private set; }
        public Ranking OutputRanking { get; private set; }

        
        public void ComputeRanking(Query query)
        {
            if (query.Equals(CachedQuery) && !InputRanking.IsUpdated)
            {
                OutputRanking.IsUpdated = false;
            }
            else
            {
                // compute partial rankings (if neccessary)
                SimilarityModule.ComputeRanking(query.SimilarityQuery);
                FilteringModule.ComputeRanking(query.FilteringQuery);

                // cache the query
                CachedQuery = query;
            }
        }
    }
}
