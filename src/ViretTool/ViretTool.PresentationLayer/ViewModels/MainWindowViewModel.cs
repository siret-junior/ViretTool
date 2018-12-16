using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Castle.Core.Logging;
using Microsoft.Win32;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Controls.Query.ViewModels;

namespace ViretTool.PresentationLayer.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly IDatasetServicesManager _datasetServicesManager;
        private readonly ILogger _logger;
        private IBiTemporalRankingService<Query, RankedResultSet, TemporalQuery, TemporalRankedResultSet> _temporalRankingService;
        private bool _isBusy;
        private bool _isFirstQueryPrimary = true;

        public MainWindowViewModel(
            ILogger logger,
            DisplayControlViewModelBase queryResults,
            ScrollDisplayControlViewModel detailView,
            QueryViewModel query1,
            QueryViewModel query2,
            IDatasetServicesManager datasetServicesManager)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;

            QueryResults = queryResults;
            DetailView = detailView;
            Query1 = query1;
            Query2 = query2;

            Query1.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();
            Query2.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();

            QueryResults.SelectedFrameChanged += async (sender, model) => await detailView.LoadVideoForFrame(model);
            QueryResults.FramesForQueryChanged += (sender, queries) => (IsFirstQueryPrimary ? Query1 : Query2).UpdateQueryObjects(queries);
            QueryResults.SortFrames += async (sender, tuple) => await detailView.LoadSortedDisplay(tuple.SelectedFrame, tuple.VisibleFrameIds);
            DetailView.FramesForQueryChanged += (sender, queries) => (IsFirstQueryPrimary ? Query1 : Query2).UpdateQueryObjects(queries);
        }

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

        public QueryViewModel Query1 { get; }
        public QueryViewModel Query2 { get; }

        public DisplayControlViewModelBase QueryResults { get; }
        public DisplayControlViewModelBase DetailView { get; }

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

        private string GetDatasetDirectory()
        {
            try
            {
                OpenFileDialog folderBrowserDialog = new OpenFileDialog
                                                     {
                                                         ValidateNames = false,
                                                         CheckFileExists = false,
                                                         CheckPathExists = true,
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

                //TODO - register properly
                _temporalRankingService = RankingServiceFactory.Build(_datasetServicesManager);

                await QueryResults.LoadInitialDisplay();

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
                TemporalRankedResultSet queryResult =
                    await Task.Run(
                        () => _temporalRankingService.ComputeRankedResultSet(
                            new TemporalQuery(IsFirstQueryPrimary ? new[] { Query1.FinalQuery, Query2.FinalQuery } : new[] { Query2.FinalQuery, Query1.FinalQuery })));

                //TODO - combine both results
                await QueryResults.LoadFramesForIds(queryResult.TemporalResultSets.First().Select(rf => rf.Id));
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
    }
}
