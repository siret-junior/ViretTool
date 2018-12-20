using System.Collections.Generic;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class KeywordQuery
    {
        public SynsetGroup[] SynsetGroups { get; private set; }
        public bool UseForSorting { get; private set; }
        public bool UseForFiltering { get; private set; }


        public KeywordQuery(SynsetGroup[] synsetGroups, bool useForSorting = true, bool useForFiltering = false)
        {
            SynsetGroups = synsetGroups;
            UseForSorting = useForSorting;
            UseForFiltering = useForFiltering;
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
