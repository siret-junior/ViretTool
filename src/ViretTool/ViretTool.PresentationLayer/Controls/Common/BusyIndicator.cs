using System.Windows;
using System.Windows.Controls;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public class BusyIndicator : ContentControl
    {

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            nameof(IsBusy),
            typeof(bool),
            typeof(BusyIndicator),
            new FrameworkPropertyMetadata(default(bool)) { BindsTwoWayByDefault = true });

        public bool IsBusy
        {
            get => (bool)GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }
    }
}
