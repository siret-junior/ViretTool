using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.Services;
using ViretTool.Core;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.Common.KeywordSearch;
using ViretTool.PresentationLayer.Controls.Common.Sketches;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Helpers;
using static ViretTool.BusinessLayer.RankingModels.Temporal.Queries.BiTemporalQuery;

namespace ViretTool.PresentationLayer.Controls.Query.ViewModels
{
    public class QueryViewModel : PropertyChangedBase
    {
        private readonly ILogger _logger;
        private readonly IInteractionLogger _interactionLogger;

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
            _interactionLogger = interationLogger;
            DatasetServicesManager = datasetServicesManager;

            datasetServicesManager.DatasetOpened += (_, services) =>
                                                    {
                                                        ImageHeight = services.DatasetParameters.DefaultFrameHeight;
                                                        ImageWidth = services.DatasetParameters.DefaultFrameWidth;
                                                    };

            PropertyChanged += (sender, args) => NotifyQuerySettingsChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender));

            QueryObjects.CollectionChanged += (sender, args) =>
                                              {
                                                  _interactionLogger.LogInteraction(
                                                      LogCategory.Image,
                                                      LogType.GlobalFeatures,
                                                      string.Join(";", QueryObjects.Select(q => q is DownloadedFrameViewModel dq ? dq.ImagePath : $"{q.VideoId}|{q.FrameNumber}")),
                                                      $"{(int)SemanticValue}%|{SemanticUseForSorting}");
                                                  SemanticUseForSorting = QueryObjects.Any();
                                                  NotifyQuerySettingsChanged(nameof(QueryObjects), args.Action);
                                              };
            InitializeScaleBitmap();
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
                _interactionLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"{BwFilterState}|{(int)BwFilterValue}%");
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
                _interactionLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"{BwFilterState}|{(int)BwFilterValue}%");
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
                bool hasColorPoints = _sketchQueryResult?.ChangedSketchTypes?.Any(type => type == SketchType.Color) == true;
                if (hasColorPoints)
                {
                    _interactionLogger.LogInteraction(
                        LogCategory.Sketch,
                        LogType.Color,
                        string.Join(",", _sketchQueryResult.SketchColorPoints.Where(p => p.SketchType == SketchType.Color)),
                        $"{(int)_colorValue}%|{ColorUseForSorting}");
                }
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
                _interactionLogger.LogInteraction(LogCategory.Text, LogType.JointEmbedding, _keywordQueryResult?.FullQuery, $"{(int)KeywordValue}%|{KeywordUseForSorting}");
                
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
                _interactionLogger.LogInteraction(LogCategory.Text, LogType.JointEmbedding, _keywordQueryResult?.FullQuery, $"{(int)_keywordValue}%|{KeywordUseForSorting}");
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
                _interactionLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"{PercentageBlackFilterState}|{(int)PercentageBlackFilterValue}%");
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
                _interactionLogger.LogInteraction(LogCategory.Filter, LogType.BW, $"{PercentageBlackFilterState}|{(int)PercentageBlackFilterValue}%");
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
                    _interactionLogger.LogInteraction(
                        LogCategory.Sketch,
                        LogType.Color,
                        string.Join(",", _sketchQueryResult.SketchColorPoints.Where(p => p.SketchType == SketchType.Color)),
                        $"{(int)ColorValue}%|{ColorUseForSorting}");

                    // use for sorting only if not already sorted by an another model
                    if (!KeywordUseForSorting && !SemanticUseForSorting)
                    {
                        ColorUseForSorting = true;
                    }
                }
                bool hasOtherPoints = _sketchQueryResult?.ChangedSketchTypes?.Any(type => type != SketchType.Color) == true;
                if (hasOtherPoints)
                {
                    _interactionLogger.LogInteraction(
                        LogCategory.Text,
                        LogType.LocalizedObject,
                        string.Join(",", _sketchQueryResult.SketchColorPoints.Where(p => p.SketchType != SketchType.Color)),
                        // text and face sketch does not use sorting or filtering
                        "");
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

        private BitmapSource _keywordToolTipBitmap = null;
        public BitmapSource KeywordToolTipBitmap
        {
            get => _keywordToolTipBitmap;
            set
            {
                if (_keywordToolTipBitmap == value)
                {
                    return;
                }
                _keywordToolTipBitmap = value;
                NotifyOfPropertyChange();
            }
        }

        private BitmapSource _colorToolTipBitmap = null;
        public BitmapSource ColorToolTipBitmap
        {
            get => _colorToolTipBitmap;
            set
            {
                if (_colorToolTipBitmap == value)
                {
                    return;
                }
                _colorToolTipBitmap = value;
                NotifyOfPropertyChange();
            }
        }

        private BitmapSource _semanticToolTipBitmap = null;
        public BitmapSource SemanticToolTipBitmap
        {
            get => _semanticToolTipBitmap;
            set
            {
                if (_semanticToolTipBitmap == value)
                {
                    return;
                }
                _semanticToolTipBitmap = value;
                NotifyOfPropertyChange();
            }
        }

        public void OnSortingExplicitlyChanged(string modelName, bool isUsedForSorting)
        {
            _interactionLogger.LogInteraction(LogCategory.Browsing, LogType.ExplicitSort, $"{modelName}:{!isUsedForSorting}");
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

        public void OnQueryResultUpdated(BiTemporalRankedResultSet queryResult)
        {
            if (!DatasetServicesManager.IsDatasetOpened)
            {
                return;
            }

            TemporalQueries primaryTemporalQuery = DatasetServicesManager.CurrentDataset.RankingService.CachedQuery.PrimaryTemporalQuery;
            IBiTemporalSimilarityModule similarityModule = DatasetServicesManager.CurrentDataset.RankingService.BiTemporalRankingModule.BiTemporalSimilarityModule;
            
            KeywordToolTipBitmap = LoadBitmapForRanking(similarityModule.KeywordModel.OutputRanking, primaryTemporalQuery, 0, 1);
            ColorToolTipBitmap = LoadBitmapForRanking(similarityModule.ColorSketchModel.OutputRanking, primaryTemporalQuery, float.MinValue, 0);
            SemanticToolTipBitmap = LoadBitmapForRanking(similarityModule.SemanticExampleModel.OutputRanking, primaryTemporalQuery);
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

        private Bitmap _canvasBitmap = new Bitmap(1000, 200);
        private Bitmap _scaleBitmap = new Bitmap(1, 200);
        private BitmapSource LoadBitmapForRanking(BiTemporalRankingBuffer ranking, TemporalQueries primaryTemporalQuery, 
            float minScore = float.MinValue, float maxScore = float.MinValue)
        {
            if (ranking == null) return null;

            // get specific temporal ranks
            float[] ranksWithFilters;
            lock (DatasetServicesManager.CurrentDataset.RankingService.Lock)
            {
                ranksWithFilters = primaryTemporalQuery == TemporalQueries.Former
                    ? ranking.FormerRankingBuffer.Ranks
                    : ranking.LatterRankingBuffer.Ranks;
            }

            // filter and sort ranks
            Array.Sort(ranksWithFilters, new Comparison<float>((i1, i2) => i2.CompareTo(i1)));
            int validRanksLength = Array.IndexOf(ranksWithFilters, float.MinValue);
            if (validRanksLength == 0) return null;
            if (validRanksLength == -1) { validRanksLength = ranksWithFilters.Length; }
            // TODO: debug
            //validRanksLength = validRanksLength < 1000 ? validRanksLength : 1000;
            float[] ranksSorted = new float[ranksWithFilters.Length];
            Array.Copy(ranksWithFilters, ranksSorted, validRanksLength);

            // update value range if necessary
            if (minScore == float.MinValue)
            {
                minScore = ranksSorted[ranksSorted.Length - 1];
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
                    float rankRatio = Math.Abs(ranksSorted[rankSampleIndex] - minScore) / range;
                    int columnHeight = (int)(rankRatio * _canvasBitmap.Height);
                    int startRow = _canvasBitmap.Height - columnHeight;
                    gfx.DrawImage(_scaleBitmap,
                        new Rectangle(iCol, startRow, 1, columnHeight),
                        new Rectangle(0, startRow, 1, columnHeight),  
                        GraphicsUnit.Pixel);
                    //gfx.DrawRectangle(Pens.Blue, iCol, startRow, 1, columnHeight);
                }

                // TODO: debug output
                //bitmap.Save($"tooltip-{ranking.Name}-{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")}.png", ImageFormat.Png);
                return _canvasBitmap.ToBitmapSource();
            }
        }

        private void InitializeScaleBitmap()
        {
            for (int iRow = 0; iRow < _scaleBitmap.Height; iRow++)
            {
                double interpolation = (double)iRow / _scaleBitmap.Height;
                Color color = ColorInterpolationHelper.InterpolateColorHSV(Color.Lime, Color.Red, interpolation, true);
                _scaleBitmap.SetPixel(0, iRow, color);
            }
        }
    }
}
