using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using Microsoft.Win32;
using Viret;
using Viret.Logging;
using Viret.Logging.DresApi;
using Viret.Ranking.ContextAware;
using Viret.Ranking.Features;
//using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
//using ViretTool.PresentationLayer.Controls.Common.LifelogFilters;
//using ViretTool.PresentationLayer.Controls.Common.TranscriptFilter;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Controls.Query.ViewModels;
using ViretTool.PresentationLayer.Helpers;
using static Viret.Ranking.RankingService;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly IDatasetServicesManager _datasetServicesManager;

        private readonly ViretCore _viretCore;

        // logging
        private readonly ILogger _logger;

        // query control
        private bool _isFirstQueryPrimary = true;
        //private readonly QueryBuilder _queryBuilder;
        
        // windows
        private readonly IWindowManager _windowManager;
        private bool _isBusy;
        private bool _isDetailViewVisible;
        private readonly TestControlViewModel _testControlViewModel;
        private string _testFramesPosition;
        private readonly SubmitControlViewModel _submitControlViewModel;
        
        // jobs
        private Task<int[]> _sortingTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        
        public MainWindowViewModel(
            ViretCore viretCore,
            ILogger logger,
            IWindowManager windowManager,
            ResultDisplayViewModel resultDisplay,
            ScrollDisplayControlViewModel detailView,
            DetailViewModel detailViewModel,
            SubmitControlViewModel submitControlViewModel,
            TestControlViewModel testControlViewModel,
            QueryViewModel query,
            IDatasetServicesManager datasetServicesManager//,
            //QueryBuilder queryBuilder
            )
        {
            _viretCore = viretCore;
            _logger = logger;
            _windowManager = windowManager;
            _submitControlViewModel = submitControlViewModel;
            _testControlViewModel = testControlViewModel;
            _datasetServicesManager = datasetServicesManager;
            //_queryBuilder = queryBuilder;
            
            ResultDisplay = resultDisplay;
            
            DetailView = detailView;
            DetailViewModel = detailViewModel;
            Query = query;
            
            // TODO: Query to textbox
            Observable.Merge(Query.QuerySettingsChanged, 
                             ResultDisplay.QueryChanged)
                      .Where(_ => !IsBusy)
                      .Throttle(TimeSpan.FromMilliseconds(50))
                      .ObserveOn(SynchronizationContext.Current)
                      .Subscribe(async _ => await OnQuerySettingsChanged());

            /**** Assign events and event handlers **************************/

            // FrameViewModel events (buttons, etc.)
            DisplayControlViewModelBase[] displays = { resultDisplay, /*somDisplay,*/ detailView, detailViewModel };
            foreach (DisplayControlViewModelBase display in displays)
            {
                display.FramesForQueryChanged += (sender, framesToQuery) => Query.UpdateQueryObjects(framesToQuery);
                display.SubmittedFramesChanged += async (sender, submittedFrames) => await OnSubmittedFramesChanged(submittedFrames);
                // TODO: unused?
                display.FrameForSimilarChanged += async (sender, selectedFrame) => await OnFrameForSimilarChanged(selectedFrame);
                display.FrameForVideoChanged += async (sender, selectedFrame) => await OnFrameForVideoChanged(selectedFrame);
                display.FrameForScrollVideoChanged += async (sender, selectedFrame) => await OnFrameForScrollVideoChanged(selectedFrame);
            }

            // Miscelaneous windows
            DetailViewModel.Close += (sender, args) => CloseDetailViewModel();
            // TODO: unused?
            _submitControlViewModel.FrameForScrollVideoChanged += async (sender, selectedFrame) => await OnFrameForScrollVideoChanged(selectedFrame);
            _testControlViewModel.Deactivated += (sender, args) => TestFramesPosition = string.Empty;
        }

        // panels
        public QueryViewModel Query { get; }
        public ResultDisplayViewModel ResultDisplay { get; }
        
        // windows
        public DisplayControlViewModelBase DetailView { get; }
        public DetailViewModel DetailViewModel { get; }

        // busy indicator selector
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                NotifyOfPropertyChange();
            }
        }


        public bool IsDetailViewVisible
        {
            get => _isDetailViewVisible;
            set
            {
                if (_isDetailViewVisible == value)
                {
                    return;
                }

                _isDetailViewVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public string TestFramesPosition
        {
            get => _testFramesPosition;
            set
            {
                if (_testFramesPosition == value)
                {
                    return;
                }

                _testFramesPosition = value;
                NotifyOfPropertyChange();
            }
        }


        // ranking model tri-selector
        private bool _isW2vvChecked = false;
        private bool _isBertChecked = false;
        private bool _isClipChecked = true;
        public bool IsW2vvChecked
        {
            get { return _isW2vvChecked; }
            set
            {
                if (_isW2vvChecked == value)
                {
                    return;
                }
                _isW2vvChecked = value;
                if (_isW2vvChecked)
                {
                    _isBertChecked = false;
                    _isClipChecked = false;
                }
                NotifyOfPropertyChange();
                OnQuerySettingsChanged();
            }
        }
        public bool IsBertChecked
        {
            get { return _isBertChecked; }
            set
            {
                if (_isBertChecked == value)
                {
                    return;
                }
                _isBertChecked = value;
                if (_isBertChecked)
                {
                    _isW2vvChecked = false;
                    _isClipChecked = false;
                }
                NotifyOfPropertyChange();
                OnQuerySettingsChanged();
            }
        }
        public bool IsClipChecked
        {
            get { return _isClipChecked; }
            set
            {
                if (_isClipChecked == value)
                {
                    return;
                }
                _isClipChecked = value;
                if (_isClipChecked)
                {
                    _isW2vvChecked = false;
                    _isBertChecked = false;
                }
                NotifyOfPropertyChange();
                OnQuerySettingsChanged();
            }
        }


        // TODO: remove? merge with OpenDataset()?
        public async void OpenDatabase()
        {
            string datasetDirectory = GetDatasetDirectory();
            if (datasetDirectory == null)
            {
                return;
            }

            await OpenDataset(datasetDirectory);
        }

        public void OpenTestWindow()
        {
            if (!_datasetServicesManager.IsDatasetOpened)
            {
                return;
            }

            _testControlViewModel.TryClose();
            _testControlViewModel.InitializeFramesRandomly();
            _windowManager.ShowWindow(_testControlViewModel);
        }

        public async void ClearAll()
        {
            IsBusy = true;
            // without this, the change is so fast that IsBusy is never triggered and user is not shown the loading overlay
            await Task.Delay(30);

            Query.OnKeywordsCleared();
            Query.OnQueryResultUpdated(null);

            // logging
            _viretCore.InteractionLogger.LogInteraction(EventCategory.Browsing, EventType.ResetAll);

            IsBusy = false;
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        // TODO: move to individual displays
        /// <summary>
        /// Navigation over the SOM/Zoom display.
        /// </summary>
        /// <param name="e"></param>
        public void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseDetailViewModel();
            }
        }


        public void OnClose(EventArgs eventArgs)
        {
            IsBusy = true;
            try
            {
                // TODO: flush logs if needed (currently not needed)
            }
            catch (Exception exception)
            {
                LogError(exception, "Error while closing application.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetDatasetDirectory()
        {
            try
            {
                OpenFileDialog folderBrowserDialog = new OpenFileDialog
                {
                    ValidateNames = false,
                    CheckFileExists = false,
                    CheckPathExists = true,
                    DereferenceLinks = false,
                    FileName = Resources.Properties.Resources.SelectDirectoryText
                };
                if (folderBrowserDialog.ShowDialog() == true)
                {
                    return Path.GetDirectoryName(folderBrowserDialog.FileName);
                }
            }
            catch (Exception e)
            {
                LogError(e, "Error while opening folder browser");
            }

            return null;
        }

        private async Task OpenDataset(string datasetFolder)
        {
            IsBusy = true;
            try
            {
                await _datasetServicesManager.OpenDatasetAsync(datasetFolder);
                if (!_datasetServicesManager.IsDatasetOpened)
                {
                    throw new DataException("Something went wrong while opening dataset.");
                }
            }
            catch (Exception e)
            {
                LogError(e, "Error while opening dataset");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void LogError(Exception e, string errorMessage)
        {
            MessageBox.Show($"{errorMessage}: {e.Message}", "Error");
            _logger.Error(errorMessage, e);
        }

        protected override void OnActivate()
        {
            _logger.Debug("Main window activated");
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                _testControlViewModel.TryClose();
            }
        }

        private async Task OnQuerySettingsChanged()
        {
            if (!_datasetServicesManager.IsDatasetOpened)
            {
                MessageBox.Show($"Please open a multimedia database (File -> Open database).", "No database loaded", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsBusy = true;
            try
            {
                CancelSortingTaskIfNecessary();

                List<AnnotatedVideoSegment> videoSegmentResults = await Task.Run(
                    () =>
                    {
                        // collect GUI query settings
                        string textualQuery = Query.KeywordQueryResult?.FullQuery ?? "";
                        string[] querySentences = textualQuery.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        RankingModel rankingModel = _isW2vvChecked ? RankingModel.W2vvBow
                                            : _isBertChecked ? RankingModel.W2vvBert
                                            : _isClipChecked ? RankingModel.Clip
                                            : throw new Exception("No ranking model is selected!");
                        string loggedTextualQueryValue = $"{Enum.GetName(typeof(RankingModel), rankingModel)}:{textualQuery}";
                        QueryEvent textualQueryEvent = new QueryEvent(EventCategory.Text, EventType.Caption, loggedTextualQueryValue);

                        // compute ranked result
                        List<VideoSegment> resultSet = _viretCore.RankingService.ComputeRankedResultSet(querySentences, rankingModel);
                        List<VideoSegment> presentedResultSet = ApplyPresentationFiltersSegmentOverlaps(resultSet);

                        // annotate
                        List<AnnotatedVideoSegment> annotatedSegments = presentedResultSet
                            .Select(segment => new AnnotatedVideoSegment(segment, querySentences)).ToList();

                        // log presented result set
                        // TODO: background task
                        List<QueryResult> resultSetLog = annotatedSegments.Select((segment, rank) => new QueryResult(
                            (_datasetServicesManager.CurrentDataset.DatasetService.GetVideoIdForFrameId(segment.SegmentFirstFrameIndex) + 1).ToString("00000"),
                            _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumberForFrameId(segment.SegmentFirstFrameIndex),
                            segment.Length,
                            segment.Score, rank)
                            ).ToList();
                        Task.Run(() => _viretCore.ResultLogger.LogResultSet(resultSetLog, textualQueryEvent, 
                            $"textSearch-{Enum.GetName(typeof(RankingModel), rankingModel)}",
                            $"top{_viretCore.Config.SegmentsInResultDisplay}SegmentsOfLength{_viretCore.Config.VideoSegmentLength}"));

                        return annotatedSegments;
                    });
                //if (videoSegmentResults.Count == 0)
                //{
                //    MessageBox.Show($"The search found no results.", "No results", MessageBoxButton.OK, MessageBoxImage.Information);
                //}

                // update test window
                List<int> sortedIds = videoSegmentResults.Select(segment => segment.SegmentFirstFrameIndex).ToList();
                UpdateTestFramesPositionIfActive(sortedIds);

                _cancellationTokenSource = new CancellationTokenSource();
                
                await ResultDisplay.LoadFramesForAnnotatedSegments(videoSegmentResults
                    .Take(_viretCore.Config.SegmentsInResultDisplay)
                    .ToList());
            }
            catch (Exception e)
            {
                LogError(e, "Error during query evaluation.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private List<VideoSegment> ApplyPresentationFiltersSegmentOverlaps(List<VideoSegment> resultSet)
        {
            // filter overlapping segments
            bool[] keyframeMask = new bool[_viretCore.Dataset.Keyframes.Count];
            List<VideoSegment> presentedResultSet = new List<VideoSegment>();
            foreach (VideoSegment segment in resultSet)
            {
                // check
                bool isOverlapping = false;
                for (int i = segment.SegmentFirstFrameIndex; i < segment.SegmentFirstFrameIndex + segment.Length; i++)
                {
                    if (keyframeMask[i])
                    {
                        isOverlapping = true;
                        break;
                    }
                }
                // add and mark
                if (!isOverlapping)
                {
                    presentedResultSet.Add(segment);
                    for (int i = segment.SegmentFirstFrameIndex; i < segment.SegmentFirstFrameIndex + segment.Length; i++)
                    {
                        keyframeMask[i] = true;
                    }
                }
            }

            return presentedResultSet;
        }

        private int[] ApplyPresentationFiltersMaxFromVideoShot(int[] resultSet)
        {
            // filter overlapping segments
            int[] videoCounter = new int[_viretCore.Dataset.Videos.Count];
            int[] shotCounter = new int[_viretCore.Dataset.Shots.Count];

            return resultSet.Where(keyframeId => 
                videoCounter[_viretCore.Dataset.Keyframes[keyframeId].ParentVideo.Id]++ < _viretCore.Config.PresentationFilterMaxFromVideo
                && shotCounter[_viretCore.Dataset.Keyframes[keyframeId].ParentShot.Id]++ < _viretCore.Config.PresentationFilterMaxFromShot
                ).ToArray();
        }

        private void UpdateTestFramesPositionIfActive(List<int> sortedIds)
        {
            if (!_testControlViewModel.IsActive)
            {
                return;
            }

            int videoId = _testControlViewModel.Frames.First().VideoId;
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            HashSet<int> frameIds = _testControlViewModel.Frames.Select(f => datasetService.GetFrameIdForFrameNumber(videoId, f.FrameNumber)).ToHashSet();
            HashSet<int> shots = frameIds.Select(id => datasetService.GetShotNumberForFrameId(id)).ToHashSet();
            int videoOrder = -1, shotOrder = -1, frameOrder = -1;
            for (int i = 0; i < sortedIds.Count; i++)
            {
                if (videoOrder == -1 && datasetService.GetVideoIdForFrameId(sortedIds[i]) == videoId)
                {
                    videoOrder = i;
                }
                if (shotOrder == -1 && shots.Contains(datasetService.GetShotNumberForFrameId(sortedIds[i])))
                {
                    shotOrder = i;
                }
                if (frameOrder == -1 && frameIds.Contains(sortedIds[i]))
                {
                    frameOrder = i;
                }
            }

            TestFramesPosition = $"{videoOrder}|{shotOrder}|{frameOrder}";
        }

        private void CancelSortingTaskIfNecessary()
        {
            if (_sortingTask != null && !_sortingTask.IsCanceled && !_sortingTask.IsCompleted)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                }
                catch (OperationCanceledException)
                {
                    //not doing anything
                }
                catch (AggregateException e)
                {
                    if (!e.InnerExceptions.All(inner => inner is OperationCanceledException))
                    {
                        throw;
                    }
                }
            }
        }

        private async Task OnSubmittedFramesChanged(IList<FrameViewModel> submittedFrames)
        {
            IsBusy = true;
            bool isDetailVisible = IsDetailViewVisible;
            IsDetailViewVisible = false;
            
            try
            {
                // open submission window
                string submittedFramesForLogging = string.Join(", ", submittedFrames.Select(f => $"{f.VideoId}|{f.FrameNumber}"));
                _viretCore.InteractionLogger.LogInteraction(EventCategory.Browsing, EventType.Exploration, $"Opened submit window: {submittedFramesForLogging}");
                _submitControlViewModel.Initialize(submittedFrames);
                if (_windowManager.ShowDialog(_submitControlViewModel) != true)
                {
                    // submission cancelled
                    _viretCore.InteractionLogger.LogInteraction(EventCategory.Browsing, EventType.Exploration, $"Cancelled submit: {submittedFramesForLogging}");
                    return;
                }
                _viretCore.InteractionLogger.LogInteraction(EventCategory.Browsing, EventType.Exploration, $"Frames submitted: {submittedFramesForLogging}");

                // submit all items
                _logger.Info($"Frames submitted: {submittedFramesForLogging}");
                foreach ((int VideoId, int FrameNumber) in _submitControlViewModel.SubmittedFrames.Select(f => (f.VideoId, f.FrameNumber)))
                {
                    try
                    {
                        _ = Task.Run(() => _viretCore.ItemSubmitter.SubmitItem(VideoId, FrameNumber))
                            .ContinueWith((t) =>
                            {
                                if (t.IsFaulted)
                                {
                                    _logger.Error($"Error submitting frame V{VideoId}, F{FrameNumber}: {t.Exception}");
                                    MessageBox.Show(t.Exception.InnerException.Message);
                                }
                            }); ;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Error submitting frame V{VideoId}, F{FrameNumber}: {ex}");
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e, "Error while submitting frames");
                return;
            }
            finally
            {
                IsBusy = isDetailVisible;
                IsDetailViewVisible = isDetailVisible;
            }
        }

        private async Task OnFrameForSimilarChanged(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            IsDetailViewVisible = true;
            _viretCore.InteractionLogger.LogInteraction(EventCategory.Image, EventType.JointEmbedding, $"{selectedFrame.VideoId}|{selectedFrame.FrameNumber}");

            int keyframeId = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(selectedFrame.VideoId, selectedFrame.FrameNumber);

            FeatureVectors featureVectors = _viretCore.FeatureVectorsBert ?? _viretCore.FeatureVectorsW2vv ?? _viretCore.FeatureVectorsClip;
            (int[] sortedFrameIds, double[] scores) = featureVectors.ComputeKnnRanking(keyframeId);

            int[] filteredSortedFrameIds = ApplyPresentationFiltersMaxFromVideoShot(sortedFrameIds);
            filteredSortedFrameIds = filteredSortedFrameIds.Take(_viretCore.Config.FramesInSimilarWindow).ToArray();
            
            // TODO: log displayed result
            QueryEvent similarQueryEvent = new QueryEvent(EventCategory.Image, EventType.JointEmbedding, $"V_{selectedFrame.VideoId}|F_{selectedFrame.FrameNumber}");
            List<QueryResult> resultSetLog = filteredSortedFrameIds.Select((frameId, rank) => new QueryResult(
                            (_datasetServicesManager.CurrentDataset.DatasetService.GetVideoIdForFrameId(frameId) + 1).ToString("00000"),
                            _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumberForFrameId(frameId),
                            1, scores[rank], rank)
                            ).ToList();
            _ = Task.Run(() => _viretCore.ResultLogger.LogResultSet(resultSetLog, similarQueryEvent, "knnToExampleImage", $"top{_viretCore.Config.FramesInSimilarWindow}"));
            await DetailViewModel.LoadSortedDisplay(selectedFrame, filteredSortedFrameIds);
        }

        private async Task OnFrameForVideoChanged(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            IsDetailViewVisible = true;
            _viretCore.InteractionLogger.LogInteraction(EventCategory.Browsing, EventType.VideoSummary, $"{selectedFrame.VideoId}|{selectedFrame.FrameNumber}");

            await DetailViewModel.LoadVideoForFrame(selectedFrame);
        }

        
        private async Task OnFrameForScrollVideoChanged(FrameViewModel selectedFrame)
        {
            await Task.Run(() => _viretCore.InteractionLogger.LogInteraction(EventCategory.Browsing, EventType.TemporalContext, $"{selectedFrame.VideoId}|{selectedFrame.FrameNumber}"));
        }

        private void CloseDetailViewModel()
        {
            _viretCore.InteractionLogger.LogInteraction(EventCategory.Browsing, DetailViewModel.BrowsingEvent, "Window closed.");
            IsBusy = false;
            IsDetailViewVisible = false;
        }
    }
}
