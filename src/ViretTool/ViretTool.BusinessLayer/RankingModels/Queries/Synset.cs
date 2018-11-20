using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class Synset
    {
        public string Text { get; private set; }
        public int SynsetId { get; private set; }

        public Synset(string text, int synsetId)
        {
            Text = text;
            SynsetId = synsetId;
        }

        public override bool Equals(object obj)
        {
            return obj is Synset keyword &&
                   SynsetId == keyword.SynsetId;
        }

        public override int GetHashCode()
        {
            int hashCode = 1397073657;
            hashCode = hashCode * -1521134295 + SynsetId.GetHashCode();
            return hashCode;
        }
    }
}
