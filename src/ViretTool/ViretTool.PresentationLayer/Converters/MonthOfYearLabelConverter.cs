using System;
using System.Globalization;
using System.Windows.Data;

namespace ViretTool.PresentationLayer.Converters
{
    public class MonthOfYearLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is int monthOfYear))
            {
                return null;
            }

            return monthOfYear;
            // TODO: text labels?
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
