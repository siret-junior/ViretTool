using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Controls.Query.ViewModels;

namespace ViretTool.PresentationLayer.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly ILogger _logger;
        private readonly IBiTemporalRankingService<Query, RankedFrame[], TemporalQuery, TemporalRankedFrame[]> _temporalRankingService;
        private string _databasePath;
        private bool _isBusy;

        public MainWindowViewModel(
            ILogger logger,
            DisplayControlViewModel displayControlViewModel,
            QueryViewModel query1,
            QueryViewModel query2,
            IBiTemporalRankingService<Query, RankedFrame[], TemporalQuery, TemporalRankedFrame[]> temporalRankingService)
        {
            _logger = logger;
            _temporalRankingService = temporalRankingService;
            DisplayControlViewModel = displayControlViewModel;

            Query1 = query1;
            Query2 = query2;

            Query1.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();
            Query2.QuerySettingsChanged += async (sender, args) => await OnQuerySettingsChanged();
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
                _logger.Error("Error during query evaluation", e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public string DatabasePath
        {
            get => _databasePath;
            set
            {
                if (_databasePath == value)
                {
                    return;
                }

                _databasePath = value;
                NotifyOfPropertyChange();
            }
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

        public DisplayControlViewModel DisplayControlViewModel { get; }
        public QueryViewModel Query1 { get; }
        public QueryViewModel Query2 { get; }

        public async void LoadDatasetButton()
        {
            if (string.IsNullOrEmpty(DatabasePath))
            {
                return;
            }

            IsBusy = true;
            await DisplayControlViewModel.LoadDataset(DatabasePath);
            IsBusy = false;
        }

        protected override void OnActivate()
        {
            _logger.Debug("Main window activated");
        }
    }
}
