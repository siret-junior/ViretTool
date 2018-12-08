using System;
using System.Globalization;
using System.Windows.Data;

namespace ViretTool.PresentationLayer.Converters
{
    public sealed class RadioEnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string current = value.ToString();
            string button = (string)parameter;

            return button == current;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string button = (string)parameter;

            if (!(bool)value)
            {
                return null;
            }

            return Enum.Parse(targetType, button);
        }
    }
}
