using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ViretTool.PresentationLayer.Converters
{
    public class BooleanToVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverted = parameter != null && System.Convert.ToBoolean(parameter);
            bool valueBool = value != null && value is bool bv && bv;

            if (inverted)
            {
                return valueBool ? Visibility.Hidden : Visibility.Visible;
            }

            return valueBool ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
