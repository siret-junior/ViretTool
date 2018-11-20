using System.Collections.Generic;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class KeywordQuery
    {
        public SynsetGroup[] SynsetGroups { get; private set; }
        

        public KeywordQuery(SynsetGroup[] synsetGroups)
        {
            SynsetGroups = synsetGroups;
        }

        public override bool Equals(object obj)
        {
            return obj is KeywordQuery query &&
                   EqualityComparer<SynsetGroup[]>.Default.Equals(SynsetGroups, query.SynsetGroups);
        }

        public override int GetHashCode()
        {
            return -1424445349 + EqualityComparer<SynsetGroup[]>.Default.GetHashCode(SynsetGroups);
        }
    }
}
