using System.Windows;
using System.Windows.Controls;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Themes
{
    class SuggestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Base { get; set; }
        public DataTemplate OnlyChildren { get; set; }
        public DataTemplate WithChildren { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (((SuggestionResultItem)item).HasOnlyChildren)
            {
                return OnlyChildren;
            }
            else if (((SuggestionResultItem)item).Hyponyms != null)
            {
                return WithChildren;
            }

            return Base;
        }
    }
}
