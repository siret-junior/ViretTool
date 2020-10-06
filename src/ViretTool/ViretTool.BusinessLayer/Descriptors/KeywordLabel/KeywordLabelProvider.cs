using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors.KeywordLabel
{
    /// <summary>
    /// Reused and modified from "ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion.LabelProvider"
    /// TODO: merge these 2 pieces of code
    /// </summary>
    public class KeywordLabelProvider : IKeywordLabelProvider<string>
    {
        public Dictionary<string, List<int>> LabelToSynsetMapping { get; }
        public Dictionary<int, string> SynsetToLabelMapping { get; }


        public KeywordLabelProvider(Dictionary<string, List<int>> labelToSynsetMapping, Dictionary<int, string> synsetToLabelMapping)
        {
            LabelToSynsetMapping = labelToSynsetMapping;
            SynsetToLabelMapping = synsetToLabelMapping;
        }
        

        public List<int> GetSynsets(string label)
        {
            return LabelToSynsetMapping[label];
        }

        // TODO: label ID numbered from 0 sequentially is used here instead of the actual synset ID -> fix that
        public string GetLabel(int synsetId)
        {
            if (SynsetToLabelMapping.TryGetValue(synsetId, out string label))
            {
                return label;
            }
            else
            {
                return $"==({synsetId}) NO LABEL==";
            }
        }
    }
}
