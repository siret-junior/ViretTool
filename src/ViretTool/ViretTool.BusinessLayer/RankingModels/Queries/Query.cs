using ViretTool.BusinessLayer.RankingModels.Filtering;
using ViretTool.BusinessLayer.RankingModels.Filtering.Queries;
using ViretTool.BusinessLayer.RankingModels.Similarity;
namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class Query
    {
        public SimilarityQuery SimilarityQuery { get; private set; }
        public FilteringQuery FilteringQuery { get; private set; }
        

        public Query(SimilarityQuery similarityQuery, FilteringQuery filteringQuery)
        {
            SimilarityQuery = similarityQuery;
            FilteringQuery = filteringQuery;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Query))
            {
                return false;
            }

            Query query = (Query)obj;
            return SimilarityQuery.Equals(query.SimilarityQuery) &&
                   FilteringQuery.Equals(query.FilteringQuery);
        }

        public override int GetHashCode()
        {
            int hashCode = 303706954;
            hashCode = hashCode * -1521134295 + SimilarityQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + FilteringQuery.GetHashCode();
            return hashCode;
        }
    }
}
