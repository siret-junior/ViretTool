using System;
using System.Globalization;
using System.Windows.Data;

namespace ViretTool.PresentationLayer.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public sealed class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double doubleType = ((double)value);

            if (parameter != null && (string)parameter == "ZeroOne")
            {
                return (doubleType / 100).ToString("0.00");
            }

            return doubleType.ToString("0.0") + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
