using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Castle.Core.Logging;
using Microsoft.Win32;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.ExternalDescriptors;
using ViretTool.BusinessLayer.OutputGridSorting;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Submission;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Controls.Query.ViewModels;
using ViretTool.PresentationLayer.Helpers;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private const int TopFramesCount = 2000;
        private readonly IDatasetServicesManager _datasetServicesManager;
        private readonly IGridSorter _gridSorter;
        private readonly ISubmissionService _submissionService;
        private readonly IInteractionLogger _interactionLogger;
        private readonly IQueryPersistingService _queryPersistingService;
        private readonly QueryBuilder _queryBuilder;
        private readonly ExternalImageProvider _externalImageProvider;
        private readonly ILogger _logger;
        private readonly IWindowManager _windowManager;
        private readonly SubmitControlViewModel _submitControlViewModel;
        private readonly TestControlViewModel _testControlViewModel;
        private bool _isBusy;
        private string _testFramesPosition;
        private bool _isDetailVisible;
        private bool _isFirstQueryPrimary = true;
        private Task<int[]> _sortingTask;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public MainWindowViewModel(
            ILogger logger,
            IWindowManager windowManager,
            PageDisplayControlViewModel queryResults,
            ScrollDisplayControlViewModel detailView,
            DetailViewModel detailViewModel,
            SubmitControlViewModel submitControlViewModel,
            TestControlViewModel testControlViewModel,
            QueryViewModel query1,
            QueryViewModel query2,
            IDatasetServicesManager datasetServicesManager,
            IGridSorter gridSorter,
            ISubmissionService submissionService,
            IInteractionLogger interactionLogger,
            IQueryPersistingService queryPersistingService,
            QueryBuilder queryBuilder,
            ExternalImageProvider externalImageProvider)
        {
            _logger = logger;
            _windowManager = windowManager;
            _submitControlViewModel = submitControlViewModel;
            _testControlViewModel = testControlViewModel;
            _datasetServicesManager = datasetServicesManager;
            _gridSorter = gridSorter;
            _submissionService = submissionService;
            _interactionLogger = interactionLogger;
            _queryPersistingService = queryPersistingService;
            _queryBuilder = queryBuilder;
            _externalImageProvider = externalImageProvider;

            QueryResults = queryResults;
            DetailView = detailView;
            DetailViewModel = detailViewModel;
            Query1 = query1;
            Query2 = query2;

            Query1.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();
            Query2.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();

            queryResults.FrameForScrollVideoChanged += async (sender, selectedFrame) => await OnFrameForScrollVideoChanged(selectedFrame);
            queryResults.MaxFramesChanged += async (sender, args) => await OnQuerySettingsChanged();
            
            DisplayControlViewModelBase[] displays = { queryResults, detailView, detailViewModel };
            foreach (var display in displays)
            {
                display.FramesForQueryChanged += (sender, queries) => (IsFirstQueryPrimary ? Query1 : Query2).UpdateQueryObjects(queries);
                display.SubmittedFramesChanged += async (sender, submittedFrames) => await OnSubmittedFramesChanged(submittedFrames);
                display.FrameForSortChanged += async (sender, selectedFrame) => await OnFrameForSortChanged(selectedFrame);
                display.FrameForVideoChanged += async (sender, selectedFrame) => await OnFrameForVideoChanged(selectedFrame);
            }

            DetailViewModel.Close += (sender, args) => CloseDetailViewModel();
            _testControlViewModel.Deactivated += (sender, args) => TestFramesPosition = string.Empty;
        }

        public QueryViewModel Query1 { get; }
        public QueryViewModel Query2 { get; }

        public PageDisplayControlViewModel QueryResults { get; }
        public DisplayControlViewModelBase DetailView { get; }
        public DetailViewModel DetailViewModel { get; }

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

        public bool IsFirstQueryPrimary
        {
            get => _isFirstQueryPrimary;
            set
            {
                if (_isFirstQueryPrimary == value)
                {
                    return;
                }
                _isFirstQueryPrimary = value;
                NotifyOfPropertyChange();
                OnQuerySettingsChanged();
            }
        }

        public bool IsDetailVisible
        {
            get => _isDetailVisible;
            set
            {
                if (_isDetailVisible == value)
                {
                    return;
                }

                _isDetailVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public int TeamId
        {
            get => _interactionLogger.Log.TeamId;
            set
            {
                if (_interactionLogger.Log.TeamId == value)
                {
                    return;
                }

                _interactionLogger.Log.TeamId = value;
                NotifyOfPropertyChange();
            }
        }

        public string SubmissionUrl
        {
            get => _submissionService.SubmissionUrl;
            set
            {
                if (_submissionService.SubmissionUrl == value)
                {
                    return;
                }

                _submissionService.SubmissionUrl = value;
                NotifyOfPropertyChange();
            }
        }

        public string TeamName
        {
            get => _interactionLogger.Log.TeamName;
            set
            {
                if (_interactionLogger.Log.TeamName == value)
                {
                    return;
                }

                _interactionLogger.Log.TeamName = value;
                NotifyOfPropertyChange();
            }
        }

        public int MemberId
        {
            get => _interactionLogger.Log.MemberId;
            set
            {
                if (_interactionLogger.Log.MemberId == value)
                {
                    return;
                }

                _interactionLogger.Log.MemberId = value;
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

        public void ClearAll()
        {
            foreach (QueryViewModel queryViewModel in new[] { Query1, Query2 })
            {
                queryViewModel.BwFilterState = FilterControl.FilterState.Off;
                queryViewModel.PercentageBlackFilterState = FilterControl.FilterState.Off;
                queryViewModel.OnKeywordsCleared();
                queryViewModel.OnQueryObjectsCleared();
                queryViewModel.OnSketchesCleared();
                queryViewModel.QueryObjects.Clear();
            }

            _interactionLogger.LogInteraction(LogCategory.Browsing, LogType.ResetAll);
        }

        public void ShowHideBwFilters()
        {
            foreach (QueryViewModel queryViewModel in new[] { Query1, Query2 })
            {
                queryViewModel.IsBwFilterVisible = !queryViewModel.IsBwFilterVisible;
            }
        }

        public async void SendLogs()
        {
            string response = await _submissionService.SubmitLog();
            _logger.Info($"Sending logs: {response}");
            MessageBox.Show(Resources.Properties.Resources.LogsWereSentText);
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseDetailViewModel();
            }
        }

        public async void OnDrop(DragEventArgs e)
        {
            //maybe throttle? sometimes it drops 2x
            IsBusy = true;
            try
            {
                string imagePath = await Task.Run(() => _externalImageProvider.ParseAndDownloadImageFromGoogle((string)e.Data.GetData(DataFormats.Text)));
                (IsFirstQueryPrimary ? Query1 : Query2).UpdateQueryObjects(new DownloadedFrameViewModel(_datasetServicesManager, imagePath));
            }
            catch (Exception exception)
            {
                LogError(exception, "Error while dropping image");
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

                await QueryResults.LoadInitialDisplay();

                //start async sorting computation
                _sortingTask = _gridSorter.GetSortedFrameIdsAsync(
                    QueryResults.GetTopFrameIds(TopFramesCount).Take(TopFramesCount).ToList(),
                    DetailViewModel.ColumnCount,
                    _cancellationTokenSource);
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
                return;
            }

            IsBusy = true;
            try
            {
                CancelSortingTaskIfNecessary();

                BiTemporalRankedResultSet queryResult = await Task.Run(
                                                            () =>
                                                            {
                                                                BiTemporalQuery biTemporalQuery = _queryBuilder.BuildQuery(
                                                                    Query1,
                                                                    Query2,
                                                                    IsFirstQueryPrimary,
                                                                    QueryResults.MaxFramesFromVideo,
                                                                    QueryResults.MaxFramesFromShot);
                                                                Task.Run(() => _queryPersistingService.SaveQuery(biTemporalQuery));
                                                                return _datasetServicesManager.CurrentDataset.RankingService.ComputeRankedResultSet(biTemporalQuery);
                                                            });

                List<int> sortedIds = (queryResult.TemporalQuery.PrimaryTemporalQuery == BiTemporalQuery.TemporalQueries.Former
                                           ? queryResult.FormerTemporalResultSet
                                           : queryResult.LatterTemporalResultSet).Select(rf => rf.Id).ToList();

                UpdateTestFramesPositionIfActive(sortedIds);

                _cancellationTokenSource = new CancellationTokenSource();
                //start async sorting computation
                _sortingTask = _gridSorter.GetSortedFrameIdsAsync(sortedIds.Take(TopFramesCount).ToList(), DetailViewModel.ColumnCount, _cancellationTokenSource);

                await QueryResults.LoadFramesForIds(sortedIds);
            }
            catch (Exception e)
            {
                LogError(e, "Error during query evaluation");
            }
            finally
            {
                IsBusy = false;
            }
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
            if (!_sortingTask.IsCanceled && !_sortingTask.IsCompleted)
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
            _submitControlViewModel.Initialize(submittedFrames);
            if (_windowManager.ShowDialog(_submitControlViewModel) != true)
            {
                return;
            }

            _logger.Info($"Frames submitted: {string.Join(",", _submitControlViewModel.SubmittedFrames.Select(f => f.FrameNumber))}");

            IsBusy = true;
            try
            {
                List<FrameToSubmit> framesToSubmit = _submitControlViewModel.SubmittedFrames.Select(f => new FrameToSubmit(f.VideoId + 1, f.FrameNumber)).ToList();
                foreach (FrameToSubmit frameToSubmit in framesToSubmit)
                {
                    string response = await _submissionService.SubmitFrameAsync(frameToSubmit);
                    _logger.Info(response);
                }
            }
            catch (Exception e)
            {
                LogError(e, "Error while submitting frames");
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task OnFrameForVideoChanged(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            IsDetailVisible = true;
            _interactionLogger.LogInteraction(LogCategory.Browsing, LogType.VideoSummary, $"{selectedFrame.VideoId}|{selectedFrame.FrameNumber}");

            await DetailViewModel.LoadVideoForFrame(selectedFrame);
        }

        private async Task OnFrameForSortChanged(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            int[] sortedIds = await _sortingTask;
            if (!sortedIds.Any())
            {
                MessageBox.Show(Resources.Properties.Resources.MapNotAvailableText);
                IsBusy = false;
                return;
            }

            _interactionLogger.LogInteraction(LogCategory.Browsing, LogType.Exploration, $"{selectedFrame.VideoId}|{selectedFrame.FrameNumber}");
            IsDetailVisible = true;
            await DetailViewModel.LoadSortedDisplay(selectedFrame, sortedIds);
        }

        private async Task OnFrameForScrollVideoChanged(FrameViewModel selectedFrame)
        {
            _interactionLogger.LogInteraction(LogCategory.Browsing, LogType.TemporalContext, $"{selectedFrame.VideoId}|{selectedFrame.FrameNumber}");
            await DetailView.LoadVideoForFrame(selectedFrame);
        }

        private void CloseDetailViewModel()
        {
            IsBusy = false;
            IsDetailVisible = false;
        }
    }
}
