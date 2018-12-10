using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Castle.Core.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;
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
        private readonly IBiTemporalRankingService<Query, RankedFrame[], TemporalQuery, TemporalRankedFrame[]> _temporalRankingService;
        private bool _isBusy;

        public MainWindowViewModel(
            ILogger logger,
            DisplayControlViewModel queryResults,
            DisplayControlViewModel videoSnapshots,
            QueryViewModel query1,
            QueryViewModel query2,
            IDatasetServicesManager datasetServicesManager,
            IBiTemporalRankingService<Query, RankedFrame[], TemporalQuery, TemporalRankedFrame[]> temporalRankingService)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            _temporalRankingService = temporalRankingService;

            QueryResults = queryResults;
            VideoSnapshots = videoSnapshots;
            Query1 = query1;
            Query2 = query2;

            Query1.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();
            Query2.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();

            QueryResults.SelectedFrameChanged += async (sender, model) => await videoSnapshots.Load(model.VideoId);
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

        public DisplayControlViewModel QueryResults { get; }
        public DisplayControlViewModel VideoSnapshots { get; }

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
                using (CommonOpenFileDialog folderBrowserDialog = new CommonOpenFileDialog { IsFolderPicker = true })
                {
                    if (folderBrowserDialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        //copy helps for some reason
                        return string.Copy(folderBrowserDialog.FileName);
                    }
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
                await Task.Run(() => _datasetServicesManager.OpenDataset(datasetFolder));
                if (!_datasetServicesManager.IsDatasetOpened)
                {
                    throw new DataException("Something went wrong while opening dataset.");
                }

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
            IsBusy = true;
            try
            {
                TemporalRankedFrame[] queryResult =
                    await Task.Run(() => _temporalRankingService.ComputeRankedResultSet(new TemporalQuery(new[] { Query1.FinalQuery, Query2.FinalQuery })));

                //TODO - visualize result
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
