using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels;
using ViretTool.PresentationLayer.Controls.Query.ViewModels;

namespace ViretTool.PresentationLayer.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly ILogger _logger;
        private string _databasePath;
        private bool _isBusy;

        public MainWindowViewModel(ILogger logger, DisplayControlViewModel displayControlViewModel, QueryViewModel query1, QueryViewModel query2)
        {
            _logger = logger;
            DisplayControlViewModel = displayControlViewModel;
            Query1 = query1;
            Query2 = query2;
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
