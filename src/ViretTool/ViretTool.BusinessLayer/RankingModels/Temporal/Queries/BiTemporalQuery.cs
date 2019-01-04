using System;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.BusinessLayer.RankingModels.Temporal.Queries
{
    public class BiTemporalQuery : IQuery, IEquatable<BiTemporalQuery>
    {
        public enum TemporalQueries { Former, Latter }
        
        public TemporalQueries PrimaryTemporalQuery { get; }

        public BiTemporalSimilarityQuery BiTemporalSimilarityQuery { get; private set; }
        public FusionQuery FormerFusionQuery { get; private set; }
        public FusionQuery LatterFusionQuery { get; private set; }
        public FilteringQuery FormerFilteringQuery { get; private set; }
        public FilteringQuery LatterFilteringQuery { get; private set; }


        public BiTemporalQuery(
            TemporalQueries primaryTemporalQuery, 
            BiTemporalSimilarityQuery biTemporalSimilarityQuery,
            FusionQuery formerFusionQuery,
            FusionQuery latterFusionQuery,
            FilteringQuery formerFilteringQuery,
            FilteringQuery latterFilteringQuery)
        {
            PrimaryTemporalQuery = primaryTemporalQuery;
            BiTemporalSimilarityQuery = biTemporalSimilarityQuery;
            FormerFusionQuery = formerFusionQuery;
            LatterFusionQuery = latterFusionQuery;
            FormerFilteringQuery = formerFilteringQuery;
            LatterFilteringQuery = latterFilteringQuery;
        }
        

        public bool Equals(BiTemporalQuery other)
        {
            return PrimaryTemporalQuery.Equals(other.PrimaryTemporalQuery) &&
                BiTemporalSimilarityQuery.Equals(other.BiTemporalSimilarityQuery) &&
                FormerFusionQuery.Equals(other.FormerFusionQuery) &&
                LatterFusionQuery.Equals(other.LatterFusionQuery) &&
                FormerFilteringQuery.Equals(other.FormerFilteringQuery) &&
                LatterFilteringQuery.Equals(other.LatterFilteringQuery);
        }
    }
}
