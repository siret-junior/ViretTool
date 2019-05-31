using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        private readonly IInteractionLogger _interationLogger;

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


        public QueryViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager, IInteractionLogger interationLogger)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            _interationLogger = interationLogger;
            _datasetServicesManager.DatasetOpened += (sender, services) => InitializeKeywordSearchMethod(_datasetServicesManager.CurrentDatasetFolder, new[] { "GoogLeNet" });

            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight);
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth);

            PropertyChanged += (sender, args) => NotifyQuerySettingsChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender));
            QueryObjects.CollectionChanged += (sender, args) => NotifyQuerySettingsChanged(nameof(QueryObjects), args.Action);

            Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                          eventHandler => QueryObjects.CollectionChanged += eventHandler,
                          eventHandler => QueryObjects.CollectionChanged -= eventHandler)
                      .Throttle(TimeSpan.FromMilliseconds(20))
                      .ObserveOn(SynchronizationContext.Current)
                      .Subscribe(
                          args =>
                          {
                              _interationLogger.LogInteraction(
                                  LogCategory.Image,
                                  LogType.GlobalFeatures,
                                  string.Join(";", QueryObjects.Select(q => q is DownloadedFrameViewModel dq ? dq.ImagePath : $"{q.VideoId}|{q.FrameNumber}")),
                                  $"{SemanticValue}|{SemanticUseForSorting}");
                              SemanticUseForSorting = QueryObjects.Any();
                              NotifyQuerySettingsChanged(nameof(QueryObjects), args.EventArgs.Action);
                          });
        }

        public ISubject<Unit> QuerySettingsChanged { get; } = new Subject<Unit>();

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
                _interationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"BW{BwFilterState}|{BwFilterValue}");
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
                _interationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"BW{BwFilterState}|{BwFilterValue}");
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
                _interationLogger.LogInteraction(LogCategory.Text, LogType.Concept, _keywordQueryResult?.FullQuery, $"{KeywordValue}|{KeywordUseForSorting}");
                
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
                _interationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"%{PercentageBlackFilterState}|{PercentageBlackFilterValue}");
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
                _interationLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"%{PercentageBlackFilterState}|{PercentageBlackFilterValue}");
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
                bool colorPoints = _sketchQueryResult?.ChangedSketchTypes?.Any(type => type == SketchType.Color) == true;
                if (colorPoints)
                {
                    _interationLogger.LogInteraction(
                        LogCategory.Sketch,
                        LogType.Color,
                        string.Join(",", _sketchQueryResult.SketchColorPoints.Where(p => p.SketchType == SketchType.Color)),
                        $"{ColorValue}|{ColorUseForSorting}");
                }
                bool otherPoints = _sketchQueryResult?.ChangedSketchTypes?.Any(type => type != SketchType.Color) == true;
                if (otherPoints)
                {
                    _interationLogger.LogInteraction(
                        LogCategory.Text,
                        LogType.LocalizedObject,
                        string.Join(",", _sketchQueryResult.SketchColorPoints.Where(p => p.SketchType != SketchType.Color)),
                        $"{ColorValue}|{ColorUseForSorting}");
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

        public void OnSortingExplicitlyChanged(string modelName, bool isUsedForSorting)
        {
            _interationLogger.LogInteraction(LogCategory.Browsing, LogType.ExplicitSort, $"{modelName}:{!isUsedForSorting}");
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
            var queriesToInsert = queries.Where(q => _datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(q.VideoId, q.FrameNumber, out _)).Select(f => f.Clone()).ToList();
            if (!QueryObjects.Any() && !queriesToInsert.Any())
            {
                return;
            }

            QueryObjects.Clear();
            QueryObjects.AddRange(queriesToInsert);
        }

        private void NotifyQuerySettingsChanged(string changedFilterName, object value)
        {
            _logger.Info($"Lifelog filters changed: ${changedFilterName}: {value}");
            QuerySettingsChanged.OnNext(Unit.Default);
        }
    }
}
