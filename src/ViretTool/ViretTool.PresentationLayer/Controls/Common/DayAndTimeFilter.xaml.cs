using System.Windows;
using System.Windows.Controls;

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
            typeof(int),
            typeof(DayAndTimeFilter),
            new FrameworkPropertyMetadata(8) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register(
            nameof(EndTime),
            typeof(int),
            typeof(DayAndTimeFilter),
            new FrameworkPropertyMetadata(12) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty HeartbeatLowProperty = DependencyProperty.Register(
            nameof(HeartbeatLow),
            typeof(int),
            typeof(DayAndTimeFilter),
            new PropertyMetadata(50));

        public static readonly DependencyProperty HeartbeatHighProperty = DependencyProperty.Register(
            nameof(HeartbeatHigh),
            typeof(int),
            typeof(DayAndTimeFilter),
            new PropertyMetadata(80));

        public int HeartbeatHigh
        {
            get => (int)GetValue(HeartbeatHighProperty);
            set => SetValue(HeartbeatHighProperty, value);
        }

        public int HeartbeatLow
        {
            get => (int)GetValue(HeartbeatLowProperty);
            set => SetValue(HeartbeatLowProperty, value);
        }

        public int StartTime
        {
            get => (int)GetValue(StartTimeProperty);
            set => SetValue(StartTimeProperty, value);
        }

        public int EndTime
        {
            get => (int)GetValue(EndTimeProperty);
            set => SetValue(EndTimeProperty, value);
        }
    }
}
