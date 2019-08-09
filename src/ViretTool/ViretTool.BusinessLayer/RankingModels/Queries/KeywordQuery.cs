using System.Collections.Generic;
using System.Linq;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class KeywordQuery : ISimilarityQuery
    {
        public SynsetClause[] SynsetFormulaCnf { get; private set; }

        public KeywordQuery(SynsetClause[] synsetFormulaCnf)
        {
            SynsetFormulaCnf = synsetFormulaCnf;
        }


        public override bool Equals(object obj)
        {
            return obj is KeywordQuery query &&
                   SynsetFormulaCnf.SequenceEqual(query.SynsetFormulaCnf);
        }

        public override int GetHashCode()
        {
            return -1424445349 + EqualityComparer<SynsetClause[]>.Default.GetHashCode(SynsetFormulaCnf);
        }
    }
}
