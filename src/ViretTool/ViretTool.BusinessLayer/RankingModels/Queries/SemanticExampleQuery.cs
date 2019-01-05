using System.Collections.Generic;
using System.Linq;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class SemanticExampleQuery : ISimilarityQuery
    {
        public int[] PositiveExampleIds { get; private set; }
        public int[] NegativeExampleIds { get; private set; }
        public string[] ExternalImages { get; }


        public SemanticExampleQuery(int[] positiveExampleIds, int[] negativeExampleIds, string[] externalImages)
        {
            PositiveExampleIds = positiveExampleIds;
            NegativeExampleIds = negativeExampleIds;
            ExternalImages = externalImages;
        }
        

        public override bool Equals(object obj)
        {
            return obj is SemanticExampleQuery query &&
                   PositiveExampleIds.SequenceEqual(query.PositiveExampleIds) &&
                   NegativeExampleIds.SequenceEqual(query.NegativeExampleIds) &&
                   ExternalImages.SequenceEqual(query.ExternalImages);
        }

        public override int GetHashCode()
        {
            int hashCode = 952631468;
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(PositiveExampleIds);
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(NegativeExampleIds);
            hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(ExternalImages);
            return hashCode;
        }
    }
}
