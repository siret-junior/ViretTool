using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Filtering
{
    public class FilteringModule : IFilteringModule
    {
        public FilteringQuery CachedQuery { get; private set; }
        public Ranking InputRanking { get; private set; }

        public Ranking OutputRanking { get; private set; }

        // TODO: add modules


        public void ComputeRanking(FilteringQuery query)
        {
            // TODO if all filters are off
            if (query.Equals(CachedQuery) && !InputRanking.IsUpdated)
            {
                OutputRanking.IsUpdated = false;
            }
            else
            {
                // TODO: filters

                // aggregate filters
                Ranking filteredRanking = null;
                // TODO:

                // cache result
                CachedQuery = query;
                OutputRanking.Ranks = filteredRanking.Ranks;
                OutputRanking.IsUpdated = true;
            }
        }

        public static Ranking ApplyFilters(Ranking ranking, bool[][] masks)
        {
            Ranking resultRanking = Ranking.Zeros(ranking.Ranks.Length);

            Parallel.For(0, ranking.Ranks.Length, itemId =>
            {
                if (ranking.Ranks[itemId] < 0)
                {
                    // ignore already filtered ranks
                    return;
                }

                for (int iMask = 0; iMask < masks.Length; iMask++)
                {
                    if (masks[iMask][itemId] == false)
                    {
                        resultRanking.Ranks[itemId] = -1;
                        return;
                    }
                }
                // not filtered, copy ranking
                resultRanking.Ranks[itemId] = ranking.Ranks[itemId];
            });

            return resultRanking;
        }
    }
}
