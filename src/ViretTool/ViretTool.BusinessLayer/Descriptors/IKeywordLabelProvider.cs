using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public interface IKeywordLabelProvider<T>
    {
        /// <summary>
        /// Mapping of label names to WordNet synset ids
        /// </summary>
        Dictionary<string, List<int>> LabelToSynsetMapping { get; }

        /// <summary>
        /// Mapping of WordNet synset ids to labels
        /// </summary>
        Dictionary<int, T> SynsetToLabelMapping { get; }

        List<int> GetSynsets(string label);
        T GetLabel(int synsetId);
        
    }
}
