using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.PresentationLayer.Controls.ViewModels;

namespace ViretTool.PresentationLayer.ViewModels
{
    public class MainWindowViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly ILogger _logger;
        private string _databasePath;
        private bool _isBusy;

        public MainWindowViewModel(ILogger logger, DisplayControlViewModel displayControlViewModel)
        {
            _logger = logger;
            DisplayControlViewModel = displayControlViewModel;
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
