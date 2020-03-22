using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion;
using System.Threading;
using System.Drawing;
using ViretTool.Core;
using System.Drawing.Imaging;
using static ViretTool.BusinessLayer.RankingModels.Temporal.Queries.BiTemporalQuery;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.PresentationLayer.Helpers;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch {
    class SuggestionTextBox : Control
    {
        static SuggestionTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SuggestionTextBox), new FrameworkPropertyMetadata(typeof(SuggestionTextBox)));
        }

        public SuggestionTextBox()
        {
            InitializeScaleBitmap();
        }



        #region Initialization

        /// <summary>
        /// Indetifies TextBox part of the UI element
        /// </summary>
        public const string PartTextBox = "PART_TextBox";
        /// <summary>
        /// Indetifies Popup (containing list of suggestions) part of the UI element
        /// </summary>
        public const string PartPopup = "PART_SuggestionPopup";

        public const string PartSourceStack = "PART_SourceStack";
        public const string PartResultStack = "PART_ResultStack";

        public IDatasetServicesManager DatasetServicesManager { get; set; }

        private TextBox TextBox_;
        private List<SuggestionPopup> Popups_;
        private WrapPanel RasultStack_;

        private List<QueryTextBlock> Query_ = new List<QueryTextBlock>();


        /// <summary>
        /// Initialize the UI elements and register events
        /// </summary>
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            TextBox_ = (TextBox)Template.FindName(PartTextBox, this);
            TextBox_.Foreground = System.Windows.Media.Brushes.Red;

            Popups_ = new List<SuggestionPopup>();
            Popups_.Add((SuggestionPopup)Template.FindName(PartPopup, this));

            TextBox_.TextChanged += TextBox_OnTextChanged;
            TextBox_.PreviewKeyDown += TextBox_OnKeyDown;
            TextBox_.LostFocus += TextBox_OnLostFocus;
            TextBox_.MouseMove += TextBox_MouseMove;

            Popups_[0].OnItemSelected += Popup_OnItemSelected;
            Popups_[0].OnItemExpanded += Popup_OnItemExpanded;
            Popups_[0].MouseLeftButtonDown += Popup_OnMouseLeft;

            RasultStack_ = (WrapPanel)Template.FindName(PartResultStack, this);

            StackPanel s = (StackPanel)Template.FindName(PartSourceStack, this);
            for (int i = 0; i < AnnotationSources.Length; i++) {
                RadioButton r = new RadioButton();
                r.Tag = AnnotationSources[i];
                r.Content = AnnotationSources[i];
                r.GroupName = "AnnotationSources";
                r.Checked += AnnotationSourceButton_Checked;
                r.Margin = new Thickness(0, 0, 10, 0);
                if (i == 0) r.IsChecked = true;
                s.Children.Add(r);
            }
        }

        #endregion

        #region Properties

        public delegate void QueryChangedHandler(string query, string annotationSource);
        public event QueryChangedHandler QueryChangedEvent;

        public delegate void SuggestionFilterChangedHandler(string filter, string annotationSource);
        public event SuggestionFilterChangedHandler SuggestionFilterChangedEvent;

        public delegate IEnumerable<IIdentifiable> GetSuggestionSubtreeHandler(IEnumerable<int> subtree, string filter, string annotationSource);
        public event GetSuggestionSubtreeHandler GetSuggestionSubtreeEvent;

        public delegate void SuggestionsNotNeededHandler();
        public event SuggestionsNotNeededHandler SuggestionsNotNeededEvent;

        public static readonly DependencyProperty AnnotationSourceProperty = DependencyProperty.Register("AnnotationSource", typeof(string), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty AnnotationSourcesProperty = DependencyProperty.Register("AnnotationSources", typeof(string[]), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(new string[0]));
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register("IsLoading", typeof(bool), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty MaxNumberOfElementsProperty = DependencyProperty.Register("MaxNumberOfElements", typeof(int), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(50));
        public static readonly DependencyProperty LoadingPlaceholderProperty = DependencyProperty.Register("LoadingPlaceholder", typeof(object), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ToolTipMessageProperty = DependencyProperty.Register("ToolTipMessage", typeof(string), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ModelToolTipBitmapProperty = DependencyProperty.Register("ModelToolTipBitmap", typeof(BitmapSource), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ToolTipBitmapProperty = DependencyProperty.Register("ToolTipBitmap", typeof(BitmapSource), typeof(SuggestionTextBox), new FrameworkPropertyMetadata(null));

        public string AnnotationSource {
            get { return (string)GetValue(AnnotationSourceProperty); }
            set { SetValue(AnnotationSourceProperty, value); }
        }

        public string[] AnnotationSources {
            get { return (string[])GetValue(AnnotationSourcesProperty); }
            set { SetValue(AnnotationSourcesProperty, value); }
        }

        /// <summary>
        /// Template for items in the ListBox
        /// </summary>
        public DataTemplateSelector ItemTemplateSelector {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Indicates if suggestions are not ready yet
        /// </summary>
        public bool IsLoading {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        /// <summary>
        /// Maximal number of elements shown in suggestions (-1 implies unlimited)
        /// </summary>
        public int MaxNumberOfElements {
            get { return (int)GetValue(MaxNumberOfElementsProperty); }
            set { SetValue(MaxNumberOfElementsProperty, value); }
        }

        /// <summary>
        /// TextBlock shown if loading any results
        /// </summary>
        public object LoadingPlaceholder {
            get { return GetValue(LoadingPlaceholderProperty); }
            set { SetValue(LoadingPlaceholderProperty, value); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Visually update suggestions
        /// </summary>
        /// <param name="suggestions"><see cref="IEnumerable{IIdentifiable}"/> of the suggestions</param>
        /// <param name="filter">A string, the suggestions are for</param>
        public void OnSuggestionResultsReady(IEnumerable<IIdentifiable> suggestions, string filter) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                if (Popups_[0].IsPopupOpen && filter == GetLastWord(TextBox_.Text)) {
                    Popups_[0].Open(MaxNumberOfElements < 0 ? suggestions : suggestions.Take(MaxNumberOfElements));
                }
            });
        }

        /// <summary>
        /// Show message in the Popup of this UI element
        /// </summary>
        public void OnShowSuggestionMessage(string message) {
            Application.Current.Dispatcher.BeginInvoke((Action)delegate {
                ((TextBlock)LoadingPlaceholder).Text = message;
            });
        }

        #endregion

        #region Control Methods

        private void Popup_Open() {
            Popups_[0].Open(null);

            SuggestionsNotNeededEvent?.Invoke();
            SuggestionFilterChangedEvent?.Invoke(GetLastWord(TextBox_.Text), AnnotationSource);
        }

        private void Popup_CloseAll() {
            SuggestionsNotNeededEvent?.Invoke();

            while (Popups_.Count > 1) {
                Popup_CloseOne();
            }
            Popup_CloseOne();
        }

        private void Popup_CloseOne() {
            Popups_[Popups_.Count - 1].Close();

            if (Popups_.Count > 1) {
                Popups_.RemoveAt(Popups_.Count - 1);
            }
        }

        #endregion

        #region Event Handling Methods

        /// <summary>
        /// Close popup and cancel any pending search for suggestions
        /// </summary>
        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e) {
            if (!IsKeyboardFocusWithin)
                Popup_CloseAll();
        }


        public string ToolTipMessage
        {
            get { return (string)GetValue(ToolTipMessageProperty); }
            set { SetValue(ToolTipMessageProperty, value); }
        }

        public BitmapSource ToolTipBitmap
        {
            get { return (BitmapSource)GetValue(ToolTipBitmapProperty); }
            set { SetValue(ToolTipBitmapProperty, value); }
        }




        private readonly object _textBoxTooltipLock = new object();
        private readonly Dictionary<string, BitmapSource> _keywordTooltipCache = new Dictionary<string, BitmapSource>();
        private string _textBoxCachedText = "";
        private int _textBoxWordStartIndex = -1;
        private int _textBoxWordEndIndex = -1;
        
        private void TextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (DatasetServicesManager == null || !DatasetServicesManager.IsDatasetOpened) return;

            if (!Monitor.TryEnter(_textBoxTooltipLock))
            {
                // allow only one thread to compute the tooltip
                return;
            }

            try
            {
                // compute hovered word boundaries
                int hoveredCharIndex = TextBox_.GetCharacterIndexFromPoint(e.GetPosition(TextBox_), true);
                (int StartIndex, int EndIndex) = GetWordIndexes(TextBox_.Text, hoveredCharIndex);

                // check whether tooltip update is necessary
                if (_textBoxCachedText.Equals(TextBox_.Text) 
                    && StartIndex == _textBoxWordStartIndex 
                    && EndIndex == _textBoxWordEndIndex
                    && ToolTipBitmap != null)
                {
                    // tooltip update not necessary
                    return;
                }
                _textBoxCachedText = TextBox_.Text;
                _textBoxWordStartIndex = StartIndex;
                _textBoxWordEndIndex = EndIndex;

                // abort if we are not hovering over a word (whitespace or an empty string)
                if (StartIndex == EndIndex || StartIndex == -1 || EndIndex == -1)
                {
                    ToolTipBitmap = null;
                    ToolTipMessage = "";
                    return;
                }

                // compute output string
                string hoveredWord = TextBox_.Text.Substring(StartIndex, EndIndex - StartIndex + 1);

                // load tooltip, either from cache or compute it
                if (!_keywordTooltipCache.TryGetValue(hoveredWord, out BitmapSource tooltipBitmap))
                {
                    tooltipBitmap = LoadBitmapForKeyword(hoveredWord, 0, 1);
                    if (tooltipBitmap != null)
                    {
                        _keywordTooltipCache.Add(hoveredWord, tooltipBitmap);
                    }
                }

                ToolTipMessage = $"{hoveredWord}";
                ToolTipBitmap = tooltipBitmap;
            }
            finally
            {
                Monitor.Exit(_textBoxTooltipLock);
            }
        }

        private Bitmap _canvasBitmap = new Bitmap(1000, 200);
        private Bitmap _scaleBitmap = new Bitmap(1, 200);
        private readonly int _tooltipRanksCount = 1000;
        private BitmapSource LoadBitmapForKeyword(string hoveredWord, float minScore = float.MinValue, float maxScore = float.MinValue)
        {
            if (!DatasetServicesManager.IsDatasetOpened)
            {
                return null;
            }

            // get ranks
            float[] ranksWithFilters;
            lock (DatasetServicesManager.CurrentDataset.RankingService.Lock)
            {
                if (DatasetServicesManager.CurrentDataset.RankingService.OutputRanking == null) return null;

                ranksWithFilters = DatasetServicesManager.CurrentDataset
                    .RankingService
                    .BiTemporalRankingModule
                    .BiTemporalSimilarityModule
                    .KeywordModel
                    .FormerSimilarityModel
                    .GetScoring(new string[] { hoveredWord });
            }

            // filter and sort ranks
            Array.Sort(ranksWithFilters, new Comparison<float>((i1, i2) => i2.CompareTo(i1)));
            int validRanksLength = Array.IndexOf(ranksWithFilters, float.MinValue);
            if (validRanksLength == 0) return null;
            if (validRanksLength == -1) { validRanksLength = ranksWithFilters.Length; }
            // TODO: debug
            //validRanksLength = validRanksLength < 1000 ? validRanksLength : 1000;
            //float[] ranksSorted = new float[ranksWithFilters.Length];

            float[] ranksSorted = new float[_tooltipRanksCount];
            int ranksToCopyCount = Math.Min(validRanksLength, _tooltipRanksCount);
            Array.Copy(ranksWithFilters, ranksSorted, ranksToCopyCount);

            // update value range if necessary
            if (minScore == float.MinValue)
            {
                minScore = ranksSorted[ranksToCopyCount - 1];
            }
            if (maxScore == float.MinValue)
            {
                maxScore = ranksSorted[0];
            }
            float range = maxScore - minScore;

            // compute bitmap
            using (Graphics gfx = Graphics.FromImage(_canvasBitmap))
            {
                gfx.FillRectangle(System.Drawing.Brushes.White, 0, 0, _canvasBitmap.Width, _canvasBitmap.Height);

                if (range == 0) return _canvasBitmap.ToBitmapSource();

                for (int iCol = 0; iCol < _canvasBitmap.Width; iCol++)
                {
                    int rankSampleIndex = (int)(((double)iCol / _canvasBitmap.Width) * ranksSorted.Length);
                    if (rankSampleIndex >= ranksToCopyCount) break;
                    float rankRatio = Math.Abs(ranksSorted[rankSampleIndex] - minScore) / range;
                    int columnHeight = (int)(rankRatio * _canvasBitmap.Height);
                    int startRow = _canvasBitmap.Height - columnHeight;
                    gfx.DrawImage(_scaleBitmap,
                        new Rectangle(iCol, startRow, 1, columnHeight),
                        new Rectangle(0, startRow, 1, columnHeight),
                        GraphicsUnit.Pixel);
                    //gfx.DrawRectangle(Pens.Blue, iCol, _canvasBitmap.Height - columnHeight, 1, columnHeight);
                }

                return _canvasBitmap.ToBitmapSource();
            }
        }

        private void InitializeScaleBitmap()
        {
            for (int iRow = 0; iRow < _scaleBitmap.Height; iRow++)
            {
                double interpolation = (double)iRow / _scaleBitmap.Height;
                System.Drawing.Color color = ColorInterpolationHelper.InterpolateColorHSV(
                    System.Drawing.Color.Lime, System.Drawing.Color.Red, interpolation, true);
                _scaleBitmap.SetPixel(0, iRow, color);
            }
        }

        private (int StartIndex, int EndIndex) GetWordIndexes(string inputString, int characterIndex)
        {
            if (characterIndex >= inputString.Length)
            {
                return (-1, -1);
            }

            // start on the character index
            int StartIndex = characterIndex;
            int EndIndex = characterIndex;

            // return if the hovered character is a whitespace
            if (char.IsWhiteSpace(inputString[characterIndex]))
            {
                return (characterIndex, characterIndex);
            }

            // scroll left until end of the word
            while (StartIndex > 0 && !char.IsWhiteSpace(inputString[StartIndex - 1]))
            {
                StartIndex--;
            }

            // scroll right until end of the word
            while (EndIndex < inputString.Length - 1 && !char.IsWhiteSpace(inputString[EndIndex + 1]))
            {
                EndIndex++;
            }

            return (StartIndex, EndIndex);
        }

        /// <summary>
        /// Cancel any pending search for suggestions and initiate a new one with new value
        /// </summary>
        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e) {
            
            Popup_CloseAll();

            if (TextBox_.Text.Length != 0) {
                Popup_Open();
            }
        }
        /// <summary>
        /// Manage navigation in suggestions popup and run search if pressed enter
        /// </summary>
        private void TextBox_OnKeyDown(object sender, KeyEventArgs e) {
            if (!Popups_[0].IsPopupOpen) {
                if (e.Key == Key.Enter)
                {
                    
                    QueryChangedEvent?.Invoke(TextBox_.Text, AnnotationSource);
                    //LoadGraphs();

                    e.Handled = true;
                }
                else if (e.Key == Key.Back && TextBox_.Text == string.Empty) {
                    if (Query_.Count > 0) {
                        RasultStack_.Children.Remove(Query_[Query_.Count - 1]);
                        Query_.RemoveAt(Query_.Count - 1);
                        if (Query_.Count > 0) {
                            RasultStack_.Children.Remove(Query_[Query_.Count - 1]);
                            Query_.RemoveAt(Query_.Count - 1);
                        }

                        e.Handled = true;
                        //QueryChangedEvent?.Invoke(Query_, AnnotationSource);
                    }
                } else if ((e.Key == Key.Up || e.Key == Key.Down) && TextBox_.Text != string.Empty) {
                    Popup_Open();
                    e.Handled = true;
                }
            } else {
                if (e.Key == Key.Escape) {
                    Popup_CloseAll();
                    e.Handled = true;
                } else if (e.Key == Key.Left) {
                    Popup_CloseOne();
                    e.Handled = true;
                } else {
                    Popups_[Popups_.Count - 1].Popup_OnKeyDown(sender, e);
                }
            }
        }

        private void Popup_OnItemSelected(object sender, SuggestionPopup.SelectedItemRoutedEventArgs e) {
            //IIdentifiable item = e.SelectedItem;

            //QueryTextBlock b = new QueryTextBlock(item, e.CtrlKeyPressed);

            //if (Query_.Count > 0) {
            //    QueryTextBlock c = new QueryTextBlock(TextBlockType.AND);
            //    RasultStack_.Children.Add(c);
            //    Query_.Add(c);
            //    c.MouseUp += QueryOperator_MouseUp;
            //}

            //RasultStack_.Children.Add(b);
            //Query_.Add(b);
            //b.MouseUp += QueryClass_MouseUp;


            //TextBox_.Text = string.Empty;
            //TextBox_.Text = item.TextRepresentation;
            //TextBox_.SelectionStart = TextBox_.Text.Length;
            Popup_CloseAll();

            //QueryChangedEvent?.Invoke(Query_, AnnotationSource);
            QueryChangedEvent?.Invoke(TextBox_.Text, AnnotationSource);

            e.Handled = true;
        }

        private void Popup_OnItemExpanded(object sender, SuggestionPopup.SelectedItemRoutedEventArgs e) {
            IIdentifiable item = e.SelectedItem;
            if (!item.HasChildren) return;
            e.Handled = true;

            SuggestionPopup p = new SuggestionPopup();
            p.Open(GetSuggestionSubtreeEvent?.Invoke(item.Children, TextBox_.Text, AnnotationSource));
            p.ItemTemplateSelector = ItemTemplateSelector;


            p.OnItemSelected += Popup_OnItemSelected;
            p.OnItemExpanded += Popup_OnItemExpanded;
            p.PopupBorderThickness = new Thickness(0, 1, 1, 1);
            p.MouseLeftButtonDown += Popup_OnMouseLeft;

            Popups_.Add(p);

            var actualWidth = (sender as SuggestionPopup)?.ActualWidth ?? 200;
            int numberOfPopups = (int)Math.Floor((SystemParameters.PrimaryScreenWidth - 50) / actualWidth);
            numberOfPopups = (Popups_.Count - 1) % (numberOfPopups);
            p.HorizontalOffset = actualWidth * numberOfPopups;

            ((Grid)TextBox_.Parent).Children.Add(p);
        }

        private void Popup_OnMouseLeft(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is SuggestionPopup popup))
            {
                return;
            }

            popup.Popup_OnMouseLeft(sender, e);
        }

        private void AnnotationSourceButton_Checked(object sender, RoutedEventArgs e) {
            RadioButton rb = sender as RadioButton;
            if (rb == null) return;

            ClearQuery();

            AnnotationSource = rb.Tag.ToString();
        }

        #endregion

        public void ClearQuery() {
            RasultStack_.Children.Clear();
            Query_.Clear();
            TextBox_.Text = "";
            //QueryChangedEvent?.Invoke(Query_, AnnotationSource);
            QueryChangedEvent?.Invoke("", AnnotationSource);
        }

        private void QueryClass_MouseUp(object sender, MouseButtonEventArgs e) {
            QueryTextBlock b = sender as QueryTextBlock;

            for (int i = 0; i < Query_.Count; i++) {
                if (Query_[i].Id == b.Id) {
                    if (i + 1 != Query_.Count) {
                        RasultStack_.Children.Remove(Query_[i + 1]);
                        Query_.RemoveAt(i + 1);
                    }
                    RasultStack_.Children.Remove(Query_[i]);
                    Query_.RemoveAt(i);

                    if (i != 0 && i == Query_.Count) {
                        RasultStack_.Children.Remove(Query_[i - 1]);
                        Query_.RemoveAt(i - 1);
                    }
                    break;
                }
            }

            //QueryChangedEvent?.Invoke(Query_, AnnotationSource);
        }

        private void QueryOperator_MouseUp(object sender, MouseButtonEventArgs e) {
            QueryTextBlock b = sender as QueryTextBlock;

            b.Type = b.Type == TextBlockType.AND ? TextBlockType.OR : TextBlockType.AND;
            b.Text = b.Type == TextBlockType.AND ? "AND" : "OR";

            //QueryChangedEvent?.Invoke(Query_, AnnotationSource);
        }

        private string GetLastWord(string inputString)
        {
            return inputString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
        }
    }

}
