using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ViretTool.PresentationLayer.Controls.Common
{
    /// <summary>
    /// Interaction logic for FrameControl.xaml
    /// </summary>
    public partial class FrameControl : UserControl
    {
        public static readonly RoutedEvent AddToQueryClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(AddToQueryClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));

        public static readonly RoutedEvent RemoveFromQueryClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(RemoveFromQueryClicked),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));

        public static readonly RoutedEvent FrameSelectedEvent = EventManager.RegisterRoutedEvent(
            nameof(FrameSelected),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));

        public static readonly RoutedEvent FramesSubmittedEvent = EventManager.RegisterRoutedEvent(
            nameof(FramesSubmitted),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));


        public static readonly DependencyProperty IsMouseOverFrameProperty = DependencyProperty.Register(
            nameof(IsMouseOverFrame),
            typeof(bool),
            typeof(FrameControl),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register(
            nameof(IsSelectable),
            typeof(bool),
            typeof(FrameControl),
            null);

        public static readonly DependencyProperty FrameWidthProperty = DependencyProperty.Register(
            nameof(FrameWidth),
            typeof(int),
            typeof(FrameControl),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty FrameHeightProperty = DependencyProperty.Register(
            nameof(FrameHeight),
            typeof(int),
            typeof(FrameControl),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public FrameControl()
        {
            InitializeComponent();
        }

        public bool IsMouseOverFrame
        {
            get => (bool)GetValue(IsMouseOverFrameProperty);
            private set => SetValue(IsMouseOverFrameProperty, value);
        }

        public bool IsSelectable
        {
            get => (bool)GetValue(IsSelectableProperty);
            set => SetValue(IsSelectableProperty, value);
        }

        public bool IsNotSelectable => !IsSelectable;

        public int FrameWidth
        {
            get => (int)GetValue(FrameWidthProperty);
            set => SetValue(FrameWidthProperty, value);
        }

        public int FrameWidthHalf => -FrameWidth / 2;

        public int FrameHeight
        {
            get => (int)GetValue(FrameHeightProperty);
            set => SetValue(FrameHeightProperty, value);
        }

        public event RoutedEventHandler AddToQueryClicked
        {
            add => AddHandler(AddToQueryClickedEvent, value);
            remove => RemoveHandler(AddToQueryClickedEvent, value);
        }

        public event RoutedEventHandler RemoveFromQueryClicked
        {
            add => AddHandler(RemoveFromQueryClickedEvent, value);
            remove => RemoveHandler(RemoveFromQueryClickedEvent, value);
        }

        public event RoutedEventHandler FrameSelected
        {
            add => AddHandler(FrameSelectedEvent, value);
            remove => RemoveHandler(FrameSelectedEvent, value);
        }

        public event RoutedEventHandler FramesSubmitted
        {
            add => AddHandler(FramesSubmittedEvent, value);
            remove => RemoveHandler(FramesSubmittedEvent, value);
        }



        private void FrameControl_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsSelectable)
            {
                RaiseEvent(new RoutedEventArgs(FrameSelectedEvent));
            }
        }

        private void FrameControl_OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseOverFrame = false;
        }

        private void FrameControl_OnMouseMove(object sender, MouseEventArgs e)
        {
            IsMouseOverFrame = true;
        }

        private void FrameControl_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ((FrameViewModel)DataContext).ScrollNext();
            }
            else
            {
                ((FrameViewModel)DataContext).ScrollPrevious();
            }
        }

        private void ButtonAddClicked(object sender, RoutedEventArgs e)
        {
            if (IsSelectable)
            {
                RaiseEvent(new RoutedEventArgs(AddToQueryClickedEvent));
            }
        }

        private void ButtonRemovedClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RemoveFromQueryClickedEvent));
        }

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            if (IsSelectable)
            {
                RaiseEvent(new RoutedEventArgs(FramesSubmittedEvent));
            }
        }
    }
}
