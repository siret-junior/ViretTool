using System;
using System.Collections.Generic;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion
{
    class SuggestionResultItem : IIdentifiable, IComparable<SuggestionResultItem>
    {
        public const string HIGHLIGHT_END_TAG = "$~END~$";

        public const string HIGHLIGHT_START_TAG = "$~START~$";
        public string Description { get; set; }
        public string Hyponyms { get; set; }

        public Label Label { get; set; }
        public string Name { get; set; }
        public Relevance SearchRelevance { get; set; }

        public int CompareTo(SuggestionResultItem other)
        {
            return (-1) *
                   (((int)SearchRelevance.Bonus + SearchRelevance.NameHits) * 2 / (float)Label.NameLenghtInWords + SearchRelevance.DescriptionHits).CompareTo(
                       ((int)other.SearchRelevance.Bonus + other.SearchRelevance.NameHits) * 2 / (float)other.Label.NameLenghtInWords + other.SearchRelevance.DescriptionHits);
        }

        public bool HasChildren => Label.Hyponyms != null;
        public bool HasOnlyChildren => Label.Id == -1; // is only hypernym
        public IEnumerable<int> Children => Label.Hyponyms;

        public int Id => Label.SynsetId;
        public string TextDescription => Label.Name;
        public string TextRepresentation => Label.Names[0];

        public struct Relevance
        {
            public byte NameHits { get; set; }
            public byte DescriptionHits { get; set; }
            public NameBonus Bonus { get; set; }

            public enum NameBonus : byte
            {
                None = 0,
                StartsWord = 1,
                StartsName = 2,
                StartsNameAlone = 4,
                FullName = 5,
                FullNameAlone = 10
            }
        }
    }
}
