using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.Core;

namespace ViretTool.PresentationLayer.Controls.Common.LifelogFilters
{
    public class DayOfWeekViewModel : PropertyChangedBase
    {
        public DayOfWeekViewModel(DayOfWeek dayOfWeek)
        {
            DayOfWeek = dayOfWeek;
            IsSelected = true;
        }

        public bool IsSelected { get; set; }

        public DayOfWeek DayOfWeek { get; }
    }

    public class LifelogFilterViewModel : PropertyChangedBase
    {
        private readonly ILogger _logger;

        private int _heartbeatHigh = 80;
        private int _heartbeatLow = 50;
        private int _startTimeHour = 8;
        private int _endTimeHour = 12;

        public LifelogFilterViewModel(ILogger logger)
        {
            _logger = logger;
            //all days are generated and monday is first
            Array daysOfWeek = Enum.GetValues(typeof(DayOfWeek));
            DaysOfWeek = new BindableCollection<DayOfWeekViewModel>(daysOfWeek.Cast<DayOfWeek>().OrderBy(d => ((int)d + daysOfWeek.Length - 1) % daysOfWeek.Length).Select(d => new DayOfWeekViewModel(d)));
            DaysOfWeek.ForEach(d => d.PropertyChanged += (sender, args) => NotifyFiltersChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender)));
            PropertyChanged += (sender, args) => NotifyFiltersChanged(args.PropertyName, sender.GetType().GetProperty(args.PropertyName)?.GetValue(sender));
        }
        
        public BindableCollection<DayOfWeekViewModel> DaysOfWeek { get; }

        public IEnumerable<DayOfWeek> SelectedDaysOfWeek => DaysOfWeek.Where(d => d.IsSelected).Select(d => d.DayOfWeek);

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

        private void NotifyFiltersChanged(string changedFilterName, object value)
        {
            _logger.Info($"Lifelog filters changed: ${changedFilterName}: {value}");
            FiltersChanged.OnNext(Unit.Default);
        }
    }
}
