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

        public static readonly RoutedEvent SortDisplayEvent = EventManager.RegisterRoutedEvent(
            nameof(SortDisplay),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));

        public static readonly RoutedEvent VideoDisplayEvent = EventManager.RegisterRoutedEvent(
            nameof(VideoDisplay),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FrameControl));

        public static readonly RoutedEvent ScrollVideoDisplayEvent = EventManager.RegisterRoutedEvent(
            nameof(ScrollVideoDisplay),
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

        public static readonly DependencyProperty IsClickedProperty = DependencyProperty.Register(
            nameof(IsClicked),
            typeof(bool),
            typeof(FrameControl),
            new PropertyMetadata(default(bool)));

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

        public int FrameHeight
        {
            get => (int)GetValue(FrameHeightProperty);
            set => SetValue(FrameHeightProperty, value);
        }

        public bool IsClicked
        {
            get => (bool)GetValue(IsClickedProperty);
            set => SetValue(IsClickedProperty, value);
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

        public event RoutedEventHandler SortDisplay
        {
            add => AddHandler(SortDisplayEvent, value);
            remove => RemoveHandler(SortDisplayEvent, value);
        }

        public event RoutedEventHandler VideoDisplay
        {
            add => AddHandler(VideoDisplayEvent, value);
            remove => RemoveHandler(VideoDisplayEvent, value);
        }

        public event RoutedEventHandler ScrollVideoDisplay
        {
            add => AddHandler(ScrollVideoDisplayEvent, value);
            remove => RemoveHandler(ScrollVideoDisplayEvent, value);
        }



        private void FrameControl_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    IsClicked = true;
                    break;
                case MouseButton.Right:
                    IsClicked = false;
                    break;
            }

            RaiseEvent(new RoutedEventArgs(FrameSelectedEvent));
        }

        private void FrameControl_OnMouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseOverFrame = false;
        }

        private void Popup_OnMouseLeave(object sender, MouseEventArgs e)
        {
            FrameControl_OnMouseLeave(sender, e);
            IsClicked = false;

            if (!(DataContext is FrameViewModel frameViewModel))
            {
                return;
            }

            frameViewModel.ResetFrameNumber();
        }

        private void FrameControl_OnMouseMove(object sender, MouseEventArgs e)
        {
            IsMouseOverFrame = true;
        }

        private void FrameControl_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(DataContext is FrameViewModel frameViewModel))
            {
                return;
            }

            IsClicked = true;
            RaiseEvent(new RoutedEventArgs(ScrollVideoDisplayEvent));
            if (e.Delta < 0)
            {
                frameViewModel.ScrollNext();
            }
            else if (e.Delta > 0)
            {
                frameViewModel.ScrollPrevious();
            }

            e.Handled = true;
        }

        private void ButtonAddClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(AddToQueryClickedEvent));
        }

        private void ButtonRemovedClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RemoveFromQueryClickedEvent));
        }

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(FramesSubmittedEvent));
        }

        private void SortedClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SortDisplayEvent));
        }

        private void VideoClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(VideoDisplayEvent));
        }
    }
}
