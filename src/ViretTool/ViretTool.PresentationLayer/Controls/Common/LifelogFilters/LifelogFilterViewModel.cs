using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.Core;

namespace ViretTool.PresentationLayer.Controls.Common.LifelogFilters
{
    public class DayOfWeekViewModel : PropertyChangedBase
    {
        private bool _isSelected;

        public DayOfWeekViewModel(DayOfWeek dayOfWeek)
        {
            DayOfWeek = dayOfWeek;
            IsSelected = true;
        }

        public DayOfWeek DayOfWeek { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }
    }

    public class LifelogFilterViewModel : PropertyChangedBase, INotifyDataErrorInfo
    {
        private readonly ILogger _logger;
        private readonly IInteractionLogger _interationLogger;
        private int _endTimeHour;

        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        private int _heartbeatHigh;
        private int _heartbeatLow;
        private int _startTimeHour;

        public LifelogFilterViewModel(ILogger logger, IInteractionLogger interationLogger)
        {
            _logger = logger;
            _interationLogger = interationLogger;
            //all days are generated and monday is first
            Array daysOfWeek = Enum.GetValues(typeof(DayOfWeek));
            DaysOfWeek = new BindableCollection<DayOfWeekViewModel>(
                daysOfWeek.Cast<DayOfWeek>().OrderBy(d => ((int)d + daysOfWeek.Length - 1) % daysOfWeek.Length).Select(d => new DayOfWeekViewModel(d)));
            Reset();

            DaysOfWeek.ForEach(
                d => d.PropertyChanged += (sender, args) => NotifyFiltersChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender)));
            PropertyChanged += (sender, args) => NotifyFiltersChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender));
        }

        public BindableCollection<DayOfWeekViewModel> DaysOfWeek { get; }

        public int EndTimeHour
        {
            get => _endTimeHour;
            set
            {
                if (_endTimeHour == value)
                {
                    return;
                }

                _endTimeHour = value;
                NotifyOfPropertyChange();
            }
        }

        public ISubject<Unit> FiltersChanged { get; } = new Subject<Unit>();

        public int HeartbeatHigh
        {
            get => _heartbeatHigh;
            set
            {
                if (_heartbeatHigh == value)
                {
                    return;
                }

                _heartbeatHigh = value;
                NotifyOfPropertyChange();
            }
        }

        public int HeartbeatLow
        {
            get => _heartbeatLow;
            set
            {
                if (_heartbeatLow == value)
                {
                    return;
                }

                _heartbeatLow = value;
                NotifyOfPropertyChange();
            }
        }

        public IEnumerable<DayOfWeek> SelectedDaysOfWeek => DaysOfWeek.Where(d => d.IsSelected).Select(d => d.DayOfWeek);

        public int StartTimeHour
        {
            get => _startTimeHour;
            set
            {
                if (_startTimeHour == value)
                {
                    return;
                }

                _startTimeHour = value;
                NotifyOfPropertyChange();
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (_errors.TryGetValue(propertyName, out string error))
            {
                yield return error;
            }
        }

        public bool HasErrors => _errors.Any();

        public void Reset()
        {
            DaysOfWeek.ForEach(d => d.IsSelected = true);
            StartTimeHour = 2;
            EndTimeHour = 23;
            HeartbeatLow = 00;
            HeartbeatHigh = 250;
        }

        private void NotifyFiltersChanged(string changedFilterName, object value)
        {
            _logger.Info($"Lifelog filters changed: ${changedFilterName}: {value}");
            _interationLogger.LogInteraction(LogCategory.Filter, LogType.Lifelog, $"{changedFilterName}|{value}");
            Validate();
            if (!HasErrors)
            {
                FiltersChanged.OnNext(Unit.Default);
            }
        }

        private void Validate()
        {
            if (StartTimeHour >= EndTimeHour)
            {
                _errors[nameof(EndTimeHour)] = _errors[nameof(StartTimeHour)] = Resources.Properties.Resources.StartTimeEndTimeError;
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(StartTimeHour)));
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(EndTimeHour)));
            }
            else if (_errors.ContainsKey(nameof(StartTimeHour)) || _errors.ContainsKey(nameof(EndTimeHour)))
            {
                _errors.Remove(nameof(StartTimeHour));
                _errors.Remove(nameof(EndTimeHour));
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(StartTimeHour)));
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(EndTimeHour)));
            }

            if (HeartbeatLow >= HeartbeatHigh)
            {
                _errors[nameof(HeartbeatLow)] = _errors[nameof(HeartbeatHigh)] = Resources.Properties.Resources.HeartrateError;
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(HeartbeatLow)));
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(HeartbeatHigh)));
            }
            else if (_errors.ContainsKey(nameof(HeartbeatLow)) || _errors.ContainsKey(nameof(HeartbeatHigh)))
            {
                _errors.Remove(nameof(HeartbeatLow));
                _errors.Remove(nameof(HeartbeatHigh));
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(HeartbeatLow)));
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(HeartbeatHigh)));
            }
        }
    }
}
