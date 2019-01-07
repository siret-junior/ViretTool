using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
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
        private readonly IInteractionLogger _iterationLogger;

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
        private bool _isBwFilterVisible;


        public QueryViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager, IInteractionLogger iterationLogger)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            _iterationLogger = iterationLogger;
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
            onQueriesChanged.Throttle(TimeSpan.FromMilliseconds(50))
                            .ObserveOn(SynchronizationContext.Current)
                            .Subscribe(
                                _ =>
                                {
                                    _iterationLogger.LogInteraction(
                                        LogCategory.Image,
                                        LogType.GlobalFeatures,
                                        string.Join(";", QueryObjects.Select(q => q is DownloadedFrameViewModel dq ? dq.ImagePath : $"{q.VideoId}|{q.FrameNumber}")),
                                        $"{SemanticValue}|{SemanticUseForSorting}");
                                    SemanticUseForSorting = QueryObjects.Any();
                                });

            onPropertyChanged.Merge(onQueriesChanged)
                             .Throttle(TimeSpan.FromMilliseconds(50))
                             .Where(_ => datasetServicesManager.IsDatasetOpened)
                             .ObserveOn(SynchronizationContext.Current)
                             .Subscribe(NotifyQuerySettingsChange);
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
                _iterationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"BW{BwFilterState}|{BwFilterValue}");
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
                _iterationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"BW{BwFilterState}|{BwFilterValue}");
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
                if (!string.IsNullOrEmpty(value.FullQuery))
                {
                    _iterationLogger.LogInteraction(LogCategory.Text, LogType.Concept, value.FullQuery, $"{KeywordValue}|{KeywordUseForSorting}");
                }

                KeywordUseForSorting = _keywordQueryResult?.Query?.Any() == true;
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
                _iterationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"%{PercentageBlackFilterState}|{PercentageBlackFilterValue}");
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
                _iterationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"%{PercentageBlackFilterState}|{PercentageBlackFilterValue}");
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
                bool colorPoints = value.ChangedSketchTypes.Any(type => type == SketchType.Color);
                if (colorPoints)
                {
                    _iterationLogger.LogInteraction(LogCategory.Sketch, LogType.Color, /*TODO*/"", $"{ColorValue}|{ColorUseForSorting}");
                }
                bool otherPoints = value.ChangedSketchTypes.Any(type => type != SketchType.Color);
                if (otherPoints)
                {
                    _iterationLogger.LogInteraction(LogCategory.Text, LogType.LocalizedObject, /*TODO*/"", $"{ColorValue}|{ColorUseForSorting}");
                }

                ColorUseForSorting = SketchQueryResult?.SketchColorPoints?.Any() == true;
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

        public bool IsBwFilterVisible
        {
            get => _isBwFilterVisible;
            set
            {
                if (_isBwFilterVisible == value)
                {
                    return;
                }

                _isBwFilterVisible = value;
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
            SemanticUseForSorting = QueryObjects.Any();
            ColorUseForSorting = SketchQueryResult?.SketchColorPoints?.Any() == true;
        }

        public void OnSketchesCleared()
        {
            SketchQueryResult = null;
            SemanticUseForSorting = QueryObjects.Any();
            KeywordUseForSorting = _keywordQueryResult?.Query?.Any() == true;
        }

        public void OnQueryObjectsCleared()
        {
            QueryObjects.Clear();
            ColorUseForSorting = SketchQueryResult?.SketchColorPoints?.Any() == true;
            KeywordUseForSorting = _keywordQueryResult?.Query?.Any() == true;
        }

        public void UpdateQueryObjects(DownloadedFrameViewModel downloadedFrame)
        {
            QueryObjects.Clear();
            QueryObjects.Add(downloadedFrame);
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

        private void NotifyQuerySettingsChange(string change)
        {
            _logger.Info(change);
            QuerySettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
