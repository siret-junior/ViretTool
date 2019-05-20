using System;
using System.Globalization;
using System.Windows.Data;

namespace ViretTool.PresentationLayer.Converters
{
    public class DayOfWeekLableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is DayOfWeek dayOfWeek))
            {
                return null;
            }

            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return Resources.Properties.Resources.SundayLabel;
                case DayOfWeek.Monday:
                    return Resources.Properties.Resources.MondayLabel;
                case DayOfWeek.Tuesday:
                    return Resources.Properties.Resources.TuesdayLabel;
                case DayOfWeek.Wednesday:
                    return Resources.Properties.Resources.WednesdayLabel;
                case DayOfWeek.Thursday:
                    return Resources.Properties.Resources.ThursdayLabel;
                case DayOfWeek.Friday:
                    return Resources.Properties.Resources.FridayLabel;
                case DayOfWeek.Saturday:
                    return Resources.Properties.Resources.SaturdayLabel;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
