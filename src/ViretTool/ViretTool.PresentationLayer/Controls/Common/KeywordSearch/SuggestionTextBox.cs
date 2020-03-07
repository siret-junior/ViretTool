using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch {
    class SuggestionTextBox : Control
    {
        static SuggestionTextBox() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SuggestionTextBox), new FrameworkPropertyMetadata(typeof(SuggestionTextBox)));
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
            TextBox_.Foreground = Brushes.Red;

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
        /// <summary>
        /// Indicates, over which keyword is user actually hovering, -1 if no query is loaded
        /// </summary>
        private bool _areGraphsLoaded = false;
        public string ToolTipMessage
        {
            get { return (string)GetValue(ToolTipMessageProperty); }
            set { SetValue(ToolTipMessageProperty, value); }
        }
        private int keywordNumber = -1;
        private void LoadGraphs()
        {
             
        }
        private void ClearGraphs()
        {

        }
        
        private string CalculateWord(string text, int position)
        {
            if (text.Length - 1 < position || text[position] == ' ') return null;

            int start = position;
            int end = position;
            while (text[start] != ' ' && start > 0) start--;
            while (text[end] != ' ' && end < text.Length - 1) end++;

            return text.Substring(start == 0 ? 0 : start + 1, end - start - 1);
        }

        private int _textBoxWordStartIndex = -1;
        private int _textBoxWordEndIndex = -1;
        private object _textBoxTooltipLock = new object();
        private volatile bool _isTooltipLocked = false;

        private void TextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isTooltipLocked)  // bool access is threadsafe
            {
                lock (_textBoxTooltipLock)
                {
                    // double checked locking (_isTooltipLocked has to be volatile!)
                    if (_isTooltipLocked)
                    {
                        // another thread already computing the tooltip, abort
                        return;
                    }

                    // TODO: why do we need capturing mouse?
                    TextBox_.CaptureMouse();

                    // compute word boundaries
                    int hoveredCharIndex = TextBox_.GetCharacterIndexFromPoint(e.GetPosition(TextBox_), true);
                    (int StartIndex, int EndIndex) = GetWordIndexes(TextBox_.Text, hoveredCharIndex);

                    // check whether tooltip update is necessary
                    if (StartIndex == _textBoxWordStartIndex && EndIndex == _textBoxWordEndIndex)
                    {
                        // update not necessary
                        return;
                    }
                    _textBoxWordStartIndex = StartIndex;
                    _textBoxWordEndIndex = EndIndex;

                    // check whether there is any word
                    if (StartIndex == EndIndex || StartIndex == -1 || EndIndex == -1)
                    {
                        ToolTipMessage = "";
                        return;
                    }

                    // compute output string
                    ToolTipMessage = $"{TextBox_.Text.Substring(StartIndex, EndIndex - StartIndex + 1)} ({StartIndex}, {EndIndex})";

                    //string word = CalculateWord(TextBox_.Text, );
                    //ToolTipMessage = word;

                    //Console.WriteLine(i);
                }
            }
        }

        private (int StartIndex, int EndIndex) GetWordIndexes(string inputString, int characterIndex)
        {
            if (characterIndex >= inputString.Length)
            {
                return (-1, -1);
                //throw new ArgumentOutOfRangeException(
                //    $"Character index {characterIndex} is out of range of input string of length {inputString.Length}");
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
        private void LoadQueryQualityVisualisation()
        {

        }
        /// <summary>
        /// Manage navigation in suggestions popup and run search if pressed enter
        /// </summary>
        private void TextBox_OnKeyDown(object sender, KeyEventArgs e) {
            if (!Popups_[0].IsPopupOpen) {
                if (e.Key == Key.Enter)
                {
                    QueryChangedEvent?.Invoke(TextBox_.Text, AnnotationSource);
                    LoadQueryQualityVisualisation();
                    LoadGraphs();
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
                        ClearGraphs();
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
            keywordNumber = -1;
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
