using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace ViretTool.PresentationLayer.Controls.Common.LifelogFilters
{
    public class DayOfWeekViewModel : PropertyChangedBase
    {
        public DayOfWeekViewModel(DayOfWeek dayOfWeek)
        {
            DayOfWeek = dayOfWeek;
        }

        public bool IsSelected { get; set; }

        public DayOfWeek DayOfWeek { get; }
    }

    public class LifelogFilterViewModel : PropertyChangedBase
    {
        private int _heartbeatHigh = 80;
        private int _heartbeatLow = 50;
        private int _startTimeHour = 8;
        private int _endTimeHour = 12;

        public LifelogFilterViewModel()
        {
            //all days are generated and monday is first
            Array daysOfWeek = Enum.GetValues(typeof(DayOfWeek));
            DaysOfWeek = new BindableCollection<DayOfWeekViewModel>(daysOfWeek.Cast<DayOfWeek>().OrderBy(d => ((int)d + daysOfWeek.Length - 1) % daysOfWeek.Length).Select(d => new DayOfWeekViewModel(d)));
        }

        public BindableCollection<DayOfWeekViewModel> DaysOfWeek { get; }

        public IEnumerable<DayOfWeek> SelectedDaysOfWeek => DaysOfWeek.Where(d => d.IsSelected).Select(d => d.DayOfWeek);

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
    }
}
