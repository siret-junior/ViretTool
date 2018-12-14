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
        public ISimilarityModule SimilarityModule { get; set; }
        public IFilteringModule FilteringModule { get; set; }

        public Query CachedQuery { get; private set; }
        public Ranking InputRanking { get; set; }
        public Ranking OutputRanking { get; set; }

        
        public void ComputeRanking(Query query)
        {
            if ((query == null && CachedQuery == null) || query.Equals(CachedQuery) && !InputRanking.IsUpdated)
            {
                OutputRanking.IsUpdated = false;
                return;
            }

            if (query != null)
            {
                // compute partial rankings (if neccessary)
                SimilarityModule.ComputeRanking(query.SimilarityQuery);
                FilteringModule.ComputeRanking(query.FilteringQuery);

                // cache the query
                CachedQuery = query;
            }
            else
            {
                // TODO just reassign array reference (also in submodels)
                // null query, set to 0 rank
                for (int i = 0; i < OutputRanking.Ranks.Length; i++)
                {
                    if (InputRanking.Ranks[i] == float.MinValue)
                    {
                        OutputRanking.Ranks[i] = float.MinValue;
                    }
                    else
                    {
                        OutputRanking.Ranks[i] = 0;
                    }
                }
            }
        }
    }
}
