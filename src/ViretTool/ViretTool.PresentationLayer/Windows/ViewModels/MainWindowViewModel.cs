using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using Microsoft.Win32;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Datasets;
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
        private readonly QueryBuilder _queryBuilder;
        private readonly ILogger _logger;
        private readonly IWindowManager _windowManager;
        private readonly SubmitControlViewModel _submitControlViewModel;
        private bool _isBusy;
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
            QueryViewModel query1,
            QueryViewModel query2,
            IDatasetServicesManager datasetServicesManager,
            IGridSorter gridSorter,
            ISubmissionService submissionService,
            IInteractionLogger interactionLogger,
            QueryBuilder queryBuilder)
        {
            _logger = logger;
            _windowManager = windowManager;
            _submitControlViewModel = submitControlViewModel;
            _datasetServicesManager = datasetServicesManager;
            _gridSorter = gridSorter;
            _submissionService = submissionService;
            _interactionLogger = interactionLogger;
            _queryBuilder = queryBuilder;

            QueryResults = queryResults;
            DetailView = detailView;
            DetailViewModel = detailViewModel;
            Query1 = query1;
            Query2 = query2;

            Query1.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();
            Query2.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();

            queryResults.FrameForScrollVideoChanged += async (sender, selectedFrame) => await detailView.LoadVideoForFrame(selectedFrame);
            DisplayControlViewModelBase[] displays = { queryResults, detailView, detailViewModel };
            foreach (var display in displays)
            {
                display.FramesForQueryChanged += (sender, queries) => (IsFirstQueryPrimary ? Query1 : Query2).UpdateQueryObjects(queries);
                display.SubmittedFramesChanged += async (sender, submittedFrames) => await OnSubmittedFramesChanged(submittedFrames);
                display.FrameForSortChanged += async (sender, selectedFrame) => await OnFrameForSortChanged(selectedFrame);
                display.FrameForVideoChanged += async (sender, selectedFrame) => await OnFrameForVideoChanged(selectedFrame);
            }

            DetailViewModel.Close += (sender, args) => CloseDetailViewModel();
        }

        public QueryViewModel Query1 { get; }
        public QueryViewModel Query2 { get; }

        public DisplayControlViewModelBase QueryResults { get; }
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
            get => _interactionLogger.TeamId;
            set
            {
                if (_interactionLogger.TeamId == value)
                {
                    return;
                }

                _interactionLogger.TeamId = value;
                NotifyOfPropertyChange();
            }
        }

        public string TeamName
        {
            get => _interactionLogger.TeamName;
            set
            {
                if (_interactionLogger.TeamName == value)
                {
                    return;
                }

                _interactionLogger.TeamName = value;
                NotifyOfPropertyChange();
            }
        }

        public int MemberId
        {
            get => _interactionLogger.MemberId;
            set
            {
                if (_interactionLogger.MemberId == value)
                {
                    return;
                }

                _interactionLogger.MemberId = value;
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

        public void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseDetailViewModel();
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

                BiTemporalQuery biTemporalQuery = _queryBuilder.BuildQuery(
                    Query1,
                    Query2,
                    IsFirstQueryPrimary ? BiTemporalQuery.TemporalQueries.Former : BiTemporalQuery.TemporalQueries.Latter);

                BiTemporalRankedResultSet queryResult = await Task.Run(() => _datasetServicesManager.CurrentDataset.RankingService.ComputeRankedResultSet(biTemporalQuery));
                
                //TODO - combine both results
                // TODO: switch between primary and secondary (flag is in the queryResult.TemporalQuery)
                List<int> sortedIds = queryResult.FormerTemporalResultSet.Select(rf => rf.Id).ToList();

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
                IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
                List<FrameToSubmit> framesToSubmit = _submitControlViewModel
                                                     .SubmittedFrames.Select(
                                                         f => new FrameToSubmit(f.VideoId + 1, datasetService.GetFrameIdForFrameNumber(f.VideoId, f.FrameNumber), -1))
                                                     .ToList();
                foreach (FrameToSubmit frameToSubmit in framesToSubmit)
                {
                    string response = await _submissionService.SubmitFramesAsync(frameToSubmit);
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

            MessageBox.Show(Resources.Properties.Resources.FramesSentSuccessText);
        }

        private async Task OnFrameForVideoChanged(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            IsDetailVisible = true;

            await DetailViewModel.LoadVideoForFrame(selectedFrame);
        }

        private async Task OnFrameForSortChanged(FrameViewModel selectedFrame)
        {
            IsBusy = true;
            int[] sortedIds = await _sortingTask;
            IsDetailVisible = true;
            await DetailViewModel.LoadSortedDisplay(selectedFrame, sortedIds);
        }

        private void CloseDetailViewModel()
        {
            IsBusy = false;
            IsDetailVisible = false;
        }
    }
}
