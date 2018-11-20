using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class SynsetGroup
    {
        public Synset[] Synsets { get; private set; }


        public SynsetGroup(Synset[] synsets)
        {
            Synsets = synsets;
        }

        public override bool Equals(object obj)
        {
            return obj is SynsetGroup group &&
                   EqualityComparer<Synset[]>.Default.Equals(Synsets, group.Synsets);
        }

        public override int GetHashCode()
        {
            return 1603870344 + EqualityComparer<Synset[]>.Default.GetHashCode(Synsets);
        }
    }
}
