using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public partial class KeywordSearchControl : UserControl
    {
        private readonly Dictionary<string, LabelProvider> mLabelProviders = new Dictionary<string, LabelProvider>();
        private readonly Dictionary<string, SuggestionProvider> mSuggestionProviders = new Dictionary<string, SuggestionProvider>();

        public KeywordSearchControl()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            suggestionTextBox.ClearQuery();
        }

        //TODO GUI
        //public void Init(DataModel.Dataset dataset, string[] annotationSources)
        //{
        //    suggestionTextBox.AnnotationSources = annotationSources;

        //    foreach (string source in annotationSources)
        //    {
        //        string labels = dataset.GetFileNameByExtension($"-{source}.label");

        //        var labelProvider = new LabelProvider(labels);
        //        mLabelProviders.Add(source, labelProvider);

        //        var suggestionProvider = new SuggestionProvider(labelProvider);
        //        mSuggestionProviders.Add(source, suggestionProvider);
        //        suggestionProvider.SuggestionResultsReadyEvent += suggestionTextBox.OnSuggestionResultsReady;
        //        suggestionProvider.ShowSuggestionMessageEvent += suggestionTextBox.OnShowSuggestionMessage;
        //    }

        //    suggestionTextBox.QueryChangedEvent += SuggestionTextBox_QueryChangedEvent;
        //    suggestionTextBox.SuggestionFilterChangedEvent += SuggestionTextBox_SuggestionFilterChangedEvent;
        //    suggestionTextBox.SuggestionsNotNeededEvent += SuggestionTextBox_SuggestionsNotNeededEvent;
        //    suggestionTextBox.GetSuggestionSubtreeEvent += SuggestionTextBox_GetSuggestionSubtreeEvent;
        //}
        
        public static readonly DependencyProperty QueryResultProperty = DependencyProperty.Register(
            "QueryResult",
            typeof(KeywordQueryResult),
            typeof(KeywordSearchControl),
            new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true });

        public KeywordQueryResult QueryResult
        {
            get => (KeywordQueryResult)GetValue(QueryResultProperty);
            set => SetValue(QueryResultProperty, value);
        }

        private IEnumerable<IIdentifiable> SuggestionTextBox_GetSuggestionSubtreeEvent(IEnumerable<int> subtree, string filter, string annotationSource)
        {
            return mSuggestionProviders[annotationSource].GetSuggestions(subtree, filter);
        }

        private void SuggestionTextBox_QueryChangedEvent(IEnumerable<IQueryPart> query, string annotationSource)
        {
            if (annotationSource == null)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.Append("QueryChangedEvent {source:");
            sb.Append(annotationSource);
            sb.Append(",wordnet_query:");
            foreach (IQueryPart q in query)
            {
                switch (q.Type)
                {
                    case TextBlockType.Class:
                        sb.Append(q.Id);
                        sb.Append(q.UseChildren ? ",1" : ",0");
                        break;
                    case TextBlockType.OR:
                        sb.Append("or");
                        break;
                    case TextBlockType.AND:
                        sb.Append("and");
                        break;
                    default:
                        break;
                }
            }

            sb.Append("}");

            //TODO log somewhere

            List<List<int>> expanded = ExpandQuery(query, mLabelProviders[annotationSource]);
            QueryResult = new KeywordQueryResult(expanded, annotationSource);
        }

        private void SuggestionTextBox_SuggestionFilterChangedEvent(string filter, string annotationSource)
        {
            mSuggestionProviders[annotationSource].GetSuggestionsAsync(filter);
        }

        private void SuggestionTextBox_SuggestionsNotNeededEvent()
        {
            foreach (KeyValuePair<string, SuggestionProvider> provider in mSuggestionProviders)
            {
                provider.Value.CancelSuggestions();
            }
        }

        private void textClearButton_Click(object sender, RoutedEventArgs e)
        {
            suggestionTextBox.ClearQuery();
        }


        #region Parse query to list of ints

        private List<List<int>> ExpandQuery(IEnumerable<IQueryPart> query, LabelProvider lp)
        {
            var list = new List<List<int>>();
            list.Add(new List<int>());

            foreach (var item in query)
            {
                if (item.Type == TextBlockType.Class)
                {
                    if (item.UseChildren)
                    {
                        IEnumerable<int> synsetIds = ExpandLabel(new int[] { item.Id }, lp);

                        foreach (int synId in synsetIds)
                        {
                            int id = lp.Labels[synId].Id;

                            list[list.Count - 1].Add(id);
                        }
                    }
                    else
                    {
                        int id = lp.Labels[item.Id].Id;

                        list[list.Count - 1].Add(id);
                    }
                }
                else if (item.Type == TextBlockType.AND)
                {
                    list.Add(new List<int>());
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Count == 0)
                {
                    return null;
                }

                list[i] = list[i].Distinct().ToList();
            }

            return list;
        }

        private List<int> ExpandLabel(IEnumerable<int> ids, LabelProvider lp)
        {
            var list = new List<int>();
            foreach (var item in ids)
            {
                var label = lp.Labels[item];

                if (label.Id != -1)
                {
                    list.Add(label.SynsetId);
                }

                if (label.Hyponyms != null)
                {
                    list.AddRange(ExpandLabel(label.Hyponyms, lp));
                }
            }

            return list;
        }

        #endregion
    }
}
