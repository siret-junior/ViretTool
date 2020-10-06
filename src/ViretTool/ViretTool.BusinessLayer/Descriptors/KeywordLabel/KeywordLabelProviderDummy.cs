using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors.KeywordLabel
{
    public class KeywordLabelProviderDummy : IKeywordLabelProvider<string>
    {
        public Dictionary<string, List<int>> LabelToSynsetMapping { get; } = new Dictionary<string, List<int>>();
        public Dictionary<int, string> SynsetToLabelMapping { get; } = new Dictionary<int, string>();

        public string GetLabel(int synsetId)
        {
            return "-";
        }

        public List<int> GetSynsets(string label)
        {
            return new List<int>();
        }
    }
}
