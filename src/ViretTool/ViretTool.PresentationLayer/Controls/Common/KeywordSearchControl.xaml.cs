using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public partial class KeywordSearchControl : UserControl
    {
        private readonly Dictionary<string, LabelProvider> _labelProviders = new Dictionary<string, LabelProvider>();
        private readonly Dictionary<string, SuggestionProvider> _suggestionProviders = new Dictionary<string, SuggestionProvider>();

        private bool _isInitialized;

        public KeywordSearchControl()
        {
            InitializeComponent();
            //DataContext = suggestionTextBox;
        }

        public static readonly DependencyProperty QueryResultProperty = DependencyProperty.Register(
            nameof(QueryResult),
            typeof(KeywordQueryResult),
            typeof(KeywordSearchControl),
            new FrameworkPropertyMetadata(
                (obj, args) =>
                {
                    if (args.NewValue == null)
                    {
                        ((KeywordSearchControl)obj).Clear();
                    }
                }) { BindsTwoWayByDefault = true });

        public KeywordQueryResult QueryResult
        {
            get => (KeywordQueryResult)GetValue(QueryResultProperty);
            set => SetValue(QueryResultProperty, value);
        }

        public static readonly DependencyProperty DatasetServicesManagerProperty = DependencyProperty.Register(
            nameof(DatasetServicesManager),
            typeof(IDatasetServicesManager),
            typeof(KeywordSearchControl),
            new FrameworkPropertyMetadata(
                (obj, args) =>
                {
                    if (args.NewValue is IDatasetServicesManager datasetServicesManager)
                    {
                        datasetServicesManager.DatasetOpened += (_, services) => ((KeywordSearchControl)obj).Init(new[] { "GoogLeNet" }, datasetServicesManager);
                    }
                }));

        public IDatasetServicesManager DatasetServicesManager
        {
            get => (IDatasetServicesManager)GetValue(DatasetServicesManagerProperty);
            set => SetValue(DatasetServicesManagerProperty, value);
        }

        public void Clear()
        {
            suggestionTextBox.ClearQuery();
        }

        private void Init(string[] annotationSources, IDatasetServicesManager datasetServicesManager)
        {
            //unregister previous events
            foreach (SuggestionProvider suggestionProvider in _suggestionProviders.Values)
            {
                suggestionProvider.SuggestionResultsReadyEvent -= suggestionTextBox.OnSuggestionResultsReady;
                suggestionProvider.ShowSuggestionMessageEvent -= suggestionTextBox.OnShowSuggestionMessage;
            }
            _suggestionProviders.Clear();
            _labelProviders.Clear();

            suggestionTextBox.AnnotationSources = annotationSources;
            suggestionTextBox.AnnotationSource = annotationSources.First();
            suggestionTextBox.DatasetServicesManager = datasetServicesManager;

            foreach (string source in annotationSources)
            {
                //string labels = GetFileNameByExtension(datatsetDirectory, $"-{source}.label");
                //string labelsFilePath = Path.Combine(datasetDirectory, $"{Path.GetFileName(datasetDirectory)}-{source}.label");
                string labelsFilePath = Directory.GetFiles(datasetServicesManager.CurrentDatasetFolder).First(file => file.EndsWith(".label"));

                LabelProvider labelProvider = new LabelProvider(labelsFilePath);
                _labelProviders.Add(source, labelProvider);

                SuggestionProvider suggestionProvider = new SuggestionProvider(labelProvider, datasetServicesManager);
                _suggestionProviders.Add(source, suggestionProvider);
                suggestionProvider.SuggestionResultsReadyEvent += suggestionTextBox.OnSuggestionResultsReady;
                suggestionProvider.ShowSuggestionMessageEvent += suggestionTextBox.OnShowSuggestionMessage;
            }

            if (!_isInitialized)
            {
                suggestionTextBox.QueryChangedEvent += SuggestionTextBox_QueryChangedEvent;
                suggestionTextBox.SuggestionFilterChangedEvent += SuggestionTextBox_SuggestionFilterChangedEvent;
                suggestionTextBox.SuggestionsNotNeededEvent += SuggestionTextBox_SuggestionsNotNeededEvent;
                suggestionTextBox.GetSuggestionSubtreeEvent += SuggestionTextBox_GetSuggestionSubtreeEvent;
                _isInitialized = true;
            }
        }

        //private string GetFileNameByExtension(string datasetPath, string extension)
        //{
        //    string stripFilename = System.IO.Path.GetFileNameWithoutExtension(datasetPath);
        //    string modelFilename = stripFilename.Split('-')[0] + extension;
        //    string parentDirectory = System.IO.Directory.GetParent(datasetPath).ToString();

        //    return System.IO.Path.Combine(parentDirectory, modelFilename);
        //}

        private IEnumerable<IIdentifiable> SuggestionTextBox_GetSuggestionSubtreeEvent(IEnumerable<int> subtree, string filter, string annotationSource)
        {
            return _suggestionProviders[annotationSource].GetSuggestions(subtree, filter);
        }

        private void SuggestionTextBox_QueryChangedEvent(string query, string annotationSource)
        {
            if (annotationSource == null)
            {
                return;
            }

            //SynsetClause[] translatedQuery = TranslateQuery(query, mLabelProviders[annotationSource]);
            //QueryResult = new KeywordQueryResult(translatedQuery, string.Join(" ", query.Cast<TextBlock>().Select(t => t.Text)), annotationSource);
            QueryResult = new KeywordQueryResult(
                query.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries),
                query,
                annotationSource);
        }

        //private SynsetClause[] TranslateQuery(IEnumerable<IQueryPart> query, LabelProvider lp)
        //{
        //    List<List<int>> queryExanded = ExpandQuery(query, lp);
        //    if (queryExanded == null)
        //    {
        //        return new SynsetClause[0];
        //    }

        //    List<SynsetClause> resultClauses = new List<SynsetClause>();
        //    foreach (List<int> clauseList in queryExanded)
        //    {
        //        SynsetClause clause = new SynsetClause(clauseList.Select(x => new Synset(lp.Labels[x].Name, x)).ToArray());
        //        resultClauses.Add(clause);
        //    }

        //    return resultClauses.ToArray();
        //}

        private void SuggestionTextBox_SuggestionFilterChangedEvent(string filter, string annotationSource)
        {
            _suggestionProviders[annotationSource].GetSuggestionsAsync(filter);
        }

        private void SuggestionTextBox_SuggestionsNotNeededEvent()
        {
            foreach (KeyValuePair<string, SuggestionProvider> provider in _suggestionProviders)
            {
                provider.Value.CancelSuggestions();
            }
        }

        private void textClearButton_Click(object sender, RoutedEventArgs e)
        {
            suggestionTextBox.ClearQuery();
        }


        #region Parse query to list of ints

        //private List<List<int>> ExpandQuery(IEnumerable<IQueryPart> query, LabelProvider lp)
        //{
        //    List<List<int>> list = new List<List<int>>();
        //    list.Add(new List<int>());

        //    foreach (IQueryPart item in query)
        //    {
        //        if (item.Type == TextBlockType.Class)
        //        {
        //            if (item.UseChildren)
        //            {
        //                //IEnumerable<int> synsetIds = ExpandLabel(new int[] { item.Id }, lp);
        //                IEnumerable<int> synsetIds = new int[] { item.Id }; // we do not need to expand, it is precomputed

        //                foreach (int synId in synsetIds)
        //                {
        //                    int id = lp.Labels[synId].SynsetId;

        //                    list[list.Count - 1].Add(id); //new synset
        //                }
        //            }
        //            else
        //            {
        //                int id = lp.Labels[item.Id].SynsetId;

        //                list[list.Count - 1].Add(id); // new synset
        //            }
        //        }
        //        else if (item.Type == TextBlockType.AND)
        //        {
        //            list.Add(new List<int>()); //finalize clause
        //        }
        //    }

        //    for (int i = 0; i < list.Count; i++) // iterate clauses
        //    {
        //        if (list[i].Count == 0) // if a single clause is empty, return null
        //        {
        //            return null;
        //        }

        //        list[i] = list[i].Distinct().ToList(); // remove duplicates from each clause
        //    }

        //    return list; // formula is array of clauses
        //}

        //private List<int> ExpandLabel(IEnumerable<int> ids, LabelProvider lp)
        //{
        //    var list = new List<int>();
        //    foreach (var item in ids)
        //    {
        //        var label = lp.Labels[item];

        //        if (!label.IsOnlyHypernym)
        //        {
        //            list.Add(label.SynsetId);
        //        }

        //        if (label.Hyponyms != null)
        //        {
        //            list.AddRange(ExpandLabel(label.Hyponyms, lp));
        //        }
        //    }

        //    return list.Distinct().ToList();
        //}

        #endregion
    }
}
