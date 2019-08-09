using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class SynsetClause
    {
        public Synset[] SynsetLiterals { get; private set; }


        public SynsetClause(Synset[] synsetLiterals)
        {
            SynsetLiterals = synsetLiterals;
        }

        public override bool Equals(object obj)
        {
            return obj is SynsetClause clause && SynsetLiterals.SequenceEqual(clause.SynsetLiterals);
        }

        public override int GetHashCode()
        {
            return 1603870344 + EqualityComparer<Synset[]>.Default.GetHashCode(SynsetLiterals);
        }
    }
}
