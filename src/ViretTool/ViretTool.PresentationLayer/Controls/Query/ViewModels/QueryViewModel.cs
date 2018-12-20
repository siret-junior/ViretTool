using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch;
using ViretTool.PresentationLayer.Controls.Common.Sketches;

namespace ViretTool.PresentationLayer.Controls.Query.ViewModels
{
    public class QueryViewModel : PropertyChangedBase
    {
        private readonly ILogger _logger;
        private readonly IDatasetServicesManager _datasetServicesManager;

        public EventHandler QuerySettingsChanged;
        
        // TODO: load default values from a settings file
        private FilterControl.FilterState _bwFilterState = FilterControl.FilterState.Off;
        private double _bwFilterValue = 90;
        private FilterControl.FilterState _percentageBlackFilterState = FilterControl.FilterState.Off;
        private double _percentageBlackFilterValue = 65;

        private double _keywordValue = 50;
        private bool _keywordUseForSorting = false;
        private KeywordQueryResult _keywordQueryResult;

        private double _colorValue = 10;
        private bool _colorUseForSorting = false;
        private SketchQueryResult _sketchQueryResult;
        private int _canvasWidth = 240;
        private int _canvasHeight = 320;

        private double _semanticValue = 30;
        private bool _semanticUseForSorting = false;


        public QueryViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            _datasetServicesManager.DatasetOpened += (sender, services) => InitializeKeywordSearchMethod(_datasetServicesManager.CurrentDatasetFolder, new[] { "GoogLeNet" });

            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight);
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth);

            //when any property is changed, new settings are rebuild - maybe we want to throttle?
            IObservable<string> onPropertyChanged =
                Observable
                    .FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        eventHandler => PropertyChanged += eventHandler,
                        eventHandler => PropertyChanged -= eventHandler)
                    .Select(p => $"{p.EventArgs.PropertyName}: {p.Sender.GetType().GetProperty(p.EventArgs.PropertyName)?.GetValue(p.Sender)}");
            IObservable<string> onQueriesChanged = Observable
                                                   .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                                                       eventHandler => QueryObjects.CollectionChanged += eventHandler,
                                                       eventHandler => QueryObjects.CollectionChanged -= eventHandler)
                                                   .Select(p => $"{nameof(QueryObjects)}: {p.EventArgs.Action}");
            onQueriesChanged.Subscribe(_ => SemanticUseForSorting = QueryObjects.Any());

            onPropertyChanged.Merge(onQueriesChanged)
                             .Throttle(TimeSpan.FromSeconds(0.1))
                             .Where(_ => datasetServicesManager.IsDatasetOpened)
                             .ObserveOn(SynchronizationContext.Current)
                             .Subscribe(BuildQuerySettings);
        }

        public int ImageHeight { get; }
        public int ImageWidth { get; }

        public BindableCollection<FrameViewModel> QueryObjects { get; } = new BindableCollection<FrameViewModel>();

        public FilterControl.FilterState BwFilterState
        {
            get => _bwFilterState;
            set
            {
                if (_bwFilterState == value)
                {
                    return;
                }

                _bwFilterState = value;
                NotifyOfPropertyChange();
            }
        }

        public double BwFilterValue
        {
            get => _bwFilterValue;
            set
            {
                if (_bwFilterValue == value)
                {
                    return;
                }

                _bwFilterValue = value;
                NotifyOfPropertyChange();
            }
        }

        public bool ColorUseForSorting
        {
            get => _colorUseForSorting;
            set
            {
                if (_colorUseForSorting == value)
                {
                    return;
                }

                _colorUseForSorting = value;
                if (_colorUseForSorting)
                {
                    KeywordUseForSorting = false;
                    SemanticUseForSorting = false;
                }
                NotifyOfPropertyChange();
            }
        }

        public double ColorValue
        {
            get => _colorValue;
            set
            {
                if (_colorValue == value)
                {
                    return;
                }

                _colorValue = value;
                NotifyOfPropertyChange();
            }
        }

        public BusinessLayer.RankingModels.Queries.Query FinalQuery { get; private set; }

        public Action<string, string[]> InitializeKeywordSearchMethod { private get; set; }

        public KeywordQueryResult KeywordQueryResult
        {
            get => _keywordQueryResult;
            set
            {
                if (_keywordQueryResult == value)
                {
                    return;
                }

                _keywordQueryResult = value;
                KeywordUseForSorting = _keywordQueryResult?.Query.Any() == true;
                NotifyOfPropertyChange();
            }
        }

        public bool KeywordUseForSorting
        {
            get => _keywordUseForSorting;
            set
            {
                if (_keywordUseForSorting == value)
                {
                    return;
                }

                _keywordUseForSorting = value;
                if (_keywordUseForSorting)
                {
                    ColorUseForSorting = false;
                    SemanticUseForSorting = false;
                }
                NotifyOfPropertyChange();
            }
        }

        public double KeywordValue
        {
            get => _keywordValue;
            set
            {
                if (_keywordValue == value)
                {
                    return;
                }

                _keywordValue = value;
                NotifyOfPropertyChange();
            }
        }

        public FilterControl.FilterState PercentageBlackFilterState
        {
            get => _percentageBlackFilterState;
            set
            {
                if (_percentageBlackFilterState == value)
                {
                    return;
                }

                _percentageBlackFilterState = value;
                NotifyOfPropertyChange();
            }
        }

        public double PercentageBlackFilterValue
        {
            get => _percentageBlackFilterValue;
            set
            {
                if (_percentageBlackFilterValue == value)
                {
                    return;
                }

                _percentageBlackFilterValue = value;
                NotifyOfPropertyChange();
            }
        }

        public bool SemanticUseForSorting
        {
            get => _semanticUseForSorting;
            set
            {
                if (_semanticUseForSorting == value)
                {
                    return;
                }

                _semanticUseForSorting = value;
                if (_semanticUseForSorting)
                {
                    ColorUseForSorting = false;
                    KeywordUseForSorting = false;
                }
                NotifyOfPropertyChange();
            }
        }

        public double SemanticValue
        {
            get => _semanticValue;
            set
            {
                if (_semanticValue == value)
                {
                    return;
                }

                _semanticValue = value;
                NotifyOfPropertyChange();
            }
        }

        public SketchQueryResult SketchQueryResult
        {
            get => _sketchQueryResult;
            set
            {
                if (_sketchQueryResult == value)
                {
                    return;
                }

                _sketchQueryResult = value;
                ColorUseForSorting = SketchQueryResult?.SketchColorPoints.Any() == true;
                NotifyOfPropertyChange();
            }
        }

        public int CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                if (_canvasWidth == value)
                {
                    return;
                }

                _canvasWidth = value;
                NotifyOfPropertyChange();
            }
        }

        public int CanvasHeight
        {
            get => _canvasHeight;
            set
            {
                if (_canvasHeight == value)
                {
                    return;
                }

                _canvasHeight = value;
                NotifyOfPropertyChange();
            }
        }

        public void RemoveFromQueryClicked(FrameViewModel frameViewModel)
        {
            frameViewModel.IsSelectedForQuery = false;
            QueryObjects.Remove(frameViewModel);
        }

        public void OnKeywordsCleared()
        {
            KeywordQueryResult = null;
        }

        public void OnSketchesCleared()
        {
            SketchQueryResult = null;
        }

        public void OnQueryObjectsCleared()
        {
            QueryObjects.Clear();
        }

        public void UpdateQueryObjects(IList<FrameViewModel> queries)
        {
            //maybe get some feedback, what changed?
            var queriesToInsert = queries.Where(q => _datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(q.VideoId, q.FrameNumber, out _)).ToList();
            if (!QueryObjects.Any() && !queriesToInsert.Any())
            {
                return;
            }

            QueryObjects.Clear();
            QueryObjects.AddRange(queriesToInsert);
        }

        private void BuildQuerySettings(string change)
        {
            _logger.Info(change);
            //MessageBox.Show(change);

            //TODO a lot of unclear settings
            string keywordAnnotationSource = KeywordQueryResult?.AnnotationSource;
            KeywordQuery keyWordQuery = new KeywordQuery(
                KeywordQueryResult?.Query?.Select(
                        parts => new SynsetGroup(parts.Select(
                            p => new Synset(keywordAnnotationSource, p)).ToArray()))
                        .ToArray() ?? new SynsetGroup[0],
                KeywordUseForSorting,
                !KeywordUseForSorting);

            ColorSketchQuery colorSketchQuery = new ColorSketchQuery(
                CanvasWidth,
                CanvasHeight,
                SketchQueryResult?.SketchColorPoints?.Select(
                        point => new Ellipse(
                            point.Area ? Ellipse.State.All : Ellipse.State.Any,
                            point.Position.X,
                            point.Position.Y,
                            point.EllipseAxis.X,
                            point.EllipseAxis.Y,
                            0,
                            point.FillColor.R,
                            point.FillColor.G,
                            point.FillColor.B))
                        .ToArray() ?? new Ellipse[0],
                ColorUseForSorting,
                !ColorUseForSorting);

            //TODO use (e.g. color) for sorting?
            SemanticExampleQuery semanticExampleQuery = new SemanticExampleQuery(
                QueryObjects.Select(
                    q => _datasetServicesManager
                        .CurrentDataset
                        .DatasetService
                        .GetFrameIdForFrameNumber(q.VideoId, q.FrameNumber))
                        .ToArray(),
                new int[0],
                SemanticUseForSorting,
                !SemanticUseForSorting);

            FinalQuery = new BusinessLayer.RankingModels.Queries.Query(
                new SimilarityQuery(keyWordQuery, colorSketchQuery, semanticExampleQuery),
                new FilteringQuery(
                    new ThresholdFilteringQuery(ConvertToFilterState(BwFilterState), BwFilterValue * 0.01),
                    new ThresholdFilteringQuery(ConvertToFilterState(PercentageBlackFilterState), PercentageBlackFilterValue * 0.01),
                    new ThresholdFilteringQuery(ThresholdFilteringQuery.State.ExcludeAboveThreshold, ColorValue * 0.01),
                    new ThresholdFilteringQuery(ThresholdFilteringQuery.State.ExcludeAboveThreshold, KeywordValue * 0.01),
                    new ThresholdFilteringQuery(ThresholdFilteringQuery.State.ExcludeAboveThreshold, SemanticValue * 0.01)));

            QuerySettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private ThresholdFilteringQuery.State ConvertToFilterState(FilterControl.FilterState filterState)
        {
            switch (filterState)
            {
                case FilterControl.FilterState.Y:
                    return ThresholdFilteringQuery.State.ExcludeAboveThreshold;
                case FilterControl.FilterState.N:
                    return ThresholdFilteringQuery.State.IncludeAboveThreshold;
                case FilterControl.FilterState.Off:
                    return ThresholdFilteringQuery.State.Off;
                default:
                    throw new ArgumentOutOfRangeException(nameof(filterState), filterState, "Uknown filtering state.");
            }
        }
    }
}
