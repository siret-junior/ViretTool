using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ViretTool.PresentationLayer.Controls.Common
{
    /// <summary>
    /// Interaction logic for DayAndTimeFilter.xaml
    /// </summary>
    public partial class DayAndTimeFilter : UserControl
    {
        public DayAndTimeFilter()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StartTimeProperty = DependencyProperty.Register(
            nameof(StartTime),
            typeof(TimeSpan),
            typeof(DayAndTimeFilter),
            new FrameworkPropertyMetadata(new TimeSpan(8, 0, 0)) { BindsTwoWayByDefault = true });

        public TimeSpan StartTime
        {
            get => (TimeSpan)GetValue(StartTimeProperty);
            set => SetValue(StartTimeProperty, value);
        }

        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register(
            nameof(EndTime),
            typeof(TimeSpan),
            typeof(DayAndTimeFilter),
            new FrameworkPropertyMetadata(new TimeSpan(12, 0, 0)) { BindsTwoWayByDefault = true });

        public TimeSpan EndTime
        {
            get => (TimeSpan)GetValue(EndTimeProperty);
            set => SetValue(EndTimeProperty, value);
        }
    }
}
