using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.PresentationLayer.Controls.Common.TranscriptFilter
{
    public class TranscriptFilterViewModel : PropertyChangedBase
    {
        private readonly ILogger _logger;
        private readonly IInteractionLogger _interactionLogger;
        private bool _supressQueryChanged;
        public IDatasetServicesManager _datasetServicesManager { get; }

        public TranscriptFilterViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager, IInteractionLogger interationLogger)
        {
            _logger = logger;
            _interactionLogger = interationLogger;
            _datasetServicesManager = datasetServicesManager;

            PropertyChanged += (sender, args) => NotifyQuerySettingsChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender));
        }

        private string _inputText;
        public string InputText 
        {
            get => _inputText;
            set
            {
                if (_inputText == value)
                {
                    return;
                }

                _inputText = value;
                NotifyOfPropertyChange();
            }
        }

        public ISubject<Unit> QuerySettingsChanged { get; } = new Subject<Unit>();

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
