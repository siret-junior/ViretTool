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
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;

namespace ViretTool.PresentationLayer.Controls.Query.ViewModels
{
    public class QueryViewModel : PropertyChangedBase
    {
        private readonly ILogger _logger;
        private readonly IInteractionLogger _interationLogger;

        // TODO: load default values from a settings file
        private FilterControl.FilterState _bwFilterState = FilterControl.FilterState.Off;
        private double _bwFilterValue = 90;
        private FilterControl.FilterState _percentageBlackFilterState = FilterControl.FilterState.Off;
        private double _percentageBlackFilterValue = 65;

        // keyword query
        private double _keywordValue = 10;
        private bool _keywordUseForSorting = false;
        private KeywordQueryResult _keywordQueryResult;

        // color sketch query
        private double _colorValue = 10;
        private bool _colorUseForSorting = false;
        private SketchQueryResult _sketchQueryResult;
        private int _canvasWidth = 240;
        private int _canvasHeight = 320;

        // semantic example query
        private double _semanticValue = 10;
        private bool _semanticUseForSorting = false;
        private bool _isBwFilterVisible;

        private bool _supressQueryChanged;
        private int _imageHeight;
        private int _imageWidth;

        public QueryViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager, IInteractionLogger interationLogger)
        {
            _logger = logger;
            _interationLogger = interationLogger;
            DatasetServicesManager = datasetServicesManager;

            datasetServicesManager.DatasetOpened += (_, services) =>
                                                    {
                                                        ImageHeight = services.DatasetParameters.DefaultFrameHeight;
                                                        ImageWidth = services.DatasetParameters.DefaultFrameWidth;
                                                    };

            PropertyChanged += (sender, args) => NotifyQuerySettingsChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender));

            QueryObjects.CollectionChanged += (sender, args) =>
                                              {
                                                  _interationLogger.LogInteraction(
                                                      LogCategory.Image,
                                                      LogType.GlobalFeatures,
                                                      string.Join(";", QueryObjects.Select(q => q is DownloadedFrameViewModel dq ? dq.ImagePath : $"{q.VideoId}|{q.FrameNumber}")),
                                                      $"{SemanticValue}|{SemanticUseForSorting}");
                                                  SemanticUseForSorting = QueryObjects.Any();
                                                  NotifyQuerySettingsChanged(nameof(QueryObjects), args.Action);
                                              };
        }

        public ISubject<Unit> QuerySettingsChanged { get; } = new Subject<Unit>();

        public int ImageHeight
        {
            get => _imageHeight;
            private set
            {
                _imageHeight = value;
                NotifyOfPropertyChange();
            }
        }

        public int ImageWidth
        {
            get => _imageWidth;
            private set
            {
                _imageWidth = value;
                NotifyOfPropertyChange();
            }
        }

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

        public IDatasetServicesManager DatasetServicesManager { get; }

        public KeywordQueryResult KeywordQueryResult
        {
            get => _keywordQueryResult;
            set
            {
                if (_keywordQueryResult == value)
                {
                    NotifyOfPropertyChange();
                    return;
                }

                _keywordQueryResult = value;
                _interationLogger.LogInteraction(LogCategory.Text, LogType.JointEmbedding, _keywordQueryResult?.FullQuery, $"{KeywordValue}|{KeywordUseForSorting}");
                
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
                bool hasColorPoints = _sketchQueryResult?.ChangedSketchTypes?.Any(type => type == SketchType.Color) == true;
                if (hasColorPoints)
                {
                    _interationLogger.LogInteraction(
                        LogCategory.Sketch,
                        LogType.Color,
                        string.Join(",", _sketchQueryResult.SketchColorPoints.Where(p => p.SketchType == SketchType.Color)),
                        $"{ColorValue}|{ColorUseForSorting}");

                    // use for sorting only if not already sorted by an another model
                    if (!KeywordUseForSorting && !SemanticUseForSorting)
                    {
                        ColorUseForSorting = true;
                    }
                }
                bool hasOtherPoints = _sketchQueryResult?.ChangedSketchTypes?.Any(type => type != SketchType.Color) == true;
                if (hasOtherPoints)
                {
                    _interationLogger.LogInteraction(
                        LogCategory.Text,
                        LogType.LocalizedObject,
                        string.Join(",", _sketchQueryResult.SketchColorPoints.Where(p => p.SketchType != SketchType.Color)),
                        $"{ColorValue}|{ColorUseForSorting}");
                }

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

        public void UpdateQueryObjects(FramesToQuery framesToQuery)
        {
            //maybe get some feedback, what changed?
            List<FrameViewModel> queriesToInsert = framesToQuery
                                                   .Frames.Where(q => DatasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(q.VideoId, q.FrameNumber, out _))
                                                   .Select(f => f.Clone())
                                                   .ToList();
            if (!QueryObjects.Any() && !queriesToInsert.Any())
            {
                return;
            }

            _supressQueryChanged = framesToQuery.SupressResultChanges;
            QueryObjects.Clear();
            QueryObjects.AddRange(queriesToInsert);
            _supressQueryChanged = false;
        }

        private void NotifyQuerySettingsChanged(string changedFilterName, object value)
        {
            if (_supressQueryChanged)
            {
                return;
            }

            _logger.Info($"Query settings changed: ${changedFilterName}: {value}");
            QuerySettingsChanged.OnNext(Unit.Default);
        }
    }
}
