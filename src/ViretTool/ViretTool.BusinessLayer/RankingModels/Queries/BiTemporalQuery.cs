using System;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class BiTemporalQuery : IQuery, IEquatable<BiTemporalQuery>
    {
        public BiTemporalSimilarityQuery SimilarityQuery { get; private set; }
        
        public FilteringQuery FormerFilteringQuery { get; private set; }
        public FilteringQuery LatterFilteringQuery { get; private set; }


        public BiTemporalQuery(
            BiTemporalSimilarityQuery similarityQuery, 
            FilteringQuery formerFilteringQuery, 
            FilteringQuery latterFilteringQuery)
        {
            SimilarityQuery = similarityQuery;
            FormerFilteringQuery = formerFilteringQuery;
            LatterFilteringQuery = latterFilteringQuery;
        }

       
        public bool Equals(BiTemporalQuery other)
        {
            return SimilarityQuery.Equals(other.SimilarityQuery) &&
                FormerFilteringQuery.Equals(other.FormerFilteringQuery) &&
                LatterFilteringQuery.Equals(other.LatterFilteringQuery);
        }
    }
}
