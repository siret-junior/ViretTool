using System.Collections.Generic;
using System.Linq;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class KeywordQuery : ISimilarityQuery
    {
        public string[] Query { get; private set; }

        public KeywordQuery(string[] query)
        {
            Query = query;
        }


        public override bool Equals(object obj)
        {
            return obj is KeywordQuery query &&
                   Query.Equals(query.Query);
        }

        public override int GetHashCode()
        {
            return Query.GetHashCode();
        }
    }
}
