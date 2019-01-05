using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ViretTool.PresentationLayer.Controls.Common
{
    /// <summary>
    /// Interaction logic for FilterView.xaml
    /// </summary>
    public partial class FilterControl : UserControl
    {
        public delegate void FilterChangedHandler(FilterState state, double value);

        public enum FilterState
        {
            Y,
            N,
            Off
        }

        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(
            "DefaultValue",
            typeof(double),
            typeof(FilterControl),
            new FrameworkPropertyMetadata(0.0d));


        public static readonly DependencyProperty FilterNameProperty = DependencyProperty.Register(
            "FilterName",
            typeof(string),
            typeof(FilterControl),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            "State",
            typeof(FilterState),
            typeof(FilterControl),
            new FrameworkPropertyMetadata(FilterState.Off) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double),
            typeof(FilterControl),
            new FrameworkPropertyMetadata(0.0d) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty OutputValueProperty = DependencyProperty.Register(
            nameof(OutputValue),
            typeof(double),
            typeof(FilterControl),
            new FrameworkPropertyMetadata(0.0d, (obj, args) => ((FilterControl)obj).Value = (double)args.NewValue) { BindsTwoWayByDefault = true });

        public FilterControl()
        {
            InitializeComponent();
        }

        public double DefaultValue
        {
            get { return (double)GetValue(DefaultValueProperty); }
            set
            {
                SetValue(DefaultValueProperty, value);
                Value = value;
                OutputValue = value;
            }
        }

        public string FilterName
        {
            get { return (string)GetValue(FilterNameProperty); }
            set { SetValue(FilterNameProperty, value); }
        }

        public FilterState State
        {
            get { return (FilterState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double OutputValue
        {
            get { return (double)GetValue(OutputValueProperty); }
            set { SetValue(OutputValueProperty, value); }
        }

        public event FilterChangedHandler FilterChangedEvent;

        public void Reset()
        {
            Value = DefaultValue;
            State = FilterState.Off;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            FilterChangedEvent?.Invoke(State, Value / 100d);
        }

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (State == FilterState.Off)
            {
                State = FilterState.Y;
            }

            FilterChangedEvent?.Invoke(State, Value / 100d);
            OutputValue = Value;
        }
    }
}
