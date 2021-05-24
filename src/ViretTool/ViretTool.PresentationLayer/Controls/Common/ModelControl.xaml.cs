using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using ViretTool.BusinessLayer.ActionLogging;

namespace ViretTool.PresentationLayer.Controls.Common
{
    /// <summary>
    /// Interaction logic for ModelControl.xaml
    /// </summary>
    public partial class ModelControl : UserControl
    {
        public ModelControl()
        {
            InitializeComponent();
        }
        
        public static readonly RoutedEvent ModelClearedEvent = EventManager.RegisterRoutedEvent(
            nameof(ModelCleared),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));

        public static readonly RoutedEvent SortingExplicitlyChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SortingExplicitlyChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));

        public static readonly DependencyProperty ModelNameProperty = DependencyProperty.Register(
            "ModelName",
            typeof(string),
            typeof(ModelControl),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty UseForSortingProperty = DependencyProperty.Register(
            "UseForSorting",
            typeof(bool),
            typeof(ModelControl),
            new FrameworkPropertyMetadata(false) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double),
            typeof(ModelControl),
            new FrameworkPropertyMetadata(0.0d) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty OutputValueProperty = DependencyProperty.Register(
            nameof(OutputValue),
            typeof(double),
            typeof(ModelControl),
            new FrameworkPropertyMetadata(0.0d, (obj, args) => ((ModelControl)obj).Value = (double)args.NewValue) { BindsTwoWayByDefault = true });


        public event RoutedEventHandler ModelCleared
        {
            add => AddHandler(ModelClearedEvent, value);
            remove => RemoveHandler(ModelClearedEvent, value);
        }

        public event RoutedEventHandler SortingExplicitlyChanged
        {
            add => AddHandler(SortingExplicitlyChangedEvent, value);
            remove => RemoveHandler(SortingExplicitlyChangedEvent, value);
        }

        public string ModelName
        {
            get { return (string)GetValue(ModelNameProperty); }
            set { SetValue(ModelNameProperty, value); }
        }

        public bool UseForSorting
        {
            get { return (bool)GetValue(UseForSortingProperty); }
            set { SetValue(UseForSortingProperty, value); }
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

        private void Slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OutputValue = Value;
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ModelClearedEvent));
        }

        private void OnFilterClicked(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SortingExplicitlyChangedEvent));
        }
    }
}
