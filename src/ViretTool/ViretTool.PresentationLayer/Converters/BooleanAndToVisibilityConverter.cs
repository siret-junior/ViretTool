using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ViretTool.PresentationLayer.Converters
{
    public class BooleanAndToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                if (!(value is bool b) || !b)
                {
                    return Visibility.Hidden;
                }
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}
