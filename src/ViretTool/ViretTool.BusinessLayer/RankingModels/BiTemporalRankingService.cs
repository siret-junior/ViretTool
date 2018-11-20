using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class BiTemporalRankingService : ITemporalRankingService<IList<TemporalRankedFrame>>
    {
        public TemporalQuery CachedQuery { get; private set; }
        public IList<TemporalRankedFrame> CachedTemporalRanking { get; private set; }

        public IRankingService PrimaryRankingService { get; private set; }
        public IFilteringModule FilteringModule { get; private set; }


        public IList<TemporalRankedFrame> ComputeRankedResultSet(TemporalQuery query)
        {
            if (query.Equals(CachedQuery))
            {
                return CachedTemporalRanking;
            }
            else
            {
                // compute partial rankings



                // aggregate temporal rankings



                float[] ranking = SimilarityModule.ComputeRanking(query.SimilarityQuery);
                float[] temporalRanking
                    = FilteringModule.ComputeFiltering(ranking, query.FilteringQuery);

                CachedQuery = query;
                CachedTemporalRanking = temporalRanking;
                return filteredRanking;
            }
        }
    }
}
