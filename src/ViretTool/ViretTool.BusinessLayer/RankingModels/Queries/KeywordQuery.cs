using System.Collections.Generic;
using System.Linq;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class KeywordQuery : IRankingQuery
    {
        public SynsetGroup[] SynsetGroups { get; private set; }

        public KeywordQuery(SynsetGroup[] synsetGroups)
        {
            SynsetGroups = synsetGroups;
        }


        public override bool Equals(object obj)
        {
            return obj is KeywordQuery query &&
                   SynsetGroups.SequenceEqual(query.SynsetGroups);
        }

        public override int GetHashCode()
        {
            return -1424445349 + EqualityComparer<SynsetGroup[]>.Default.GetHashCode(SynsetGroups);
        }
    }
}
