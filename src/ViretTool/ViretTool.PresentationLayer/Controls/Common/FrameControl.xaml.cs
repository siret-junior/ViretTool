using System;
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

        public static readonly RoutedEvent AddToGpsQueryClickedEvent = EventManager.RegisterRoutedEvent(
            nameof(AddToGpsQueryClicked),
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

        //public static readonly RoutedEvent SortDisplayEvent = EventManager.RegisterRoutedEvent(
        //    nameof(SortDisplay),
        //    RoutingStrategy.Bubble,
        //    typeof(RoutedEventHandler),
        //    typeof(FrameControl));

        //public static readonly RoutedEvent ZoomIntoDisplayEvent = EventManager.RegisterRoutedEvent(
        //    nameof(ZoomIntoDisplay),
        //    RoutingStrategy.Bubble,
        //    typeof(RoutedEventHandler),
        //    typeof(FrameControl));

        //public static readonly RoutedEvent ZoomOutDisplayEvent = EventManager.RegisterRoutedEvent(
        //    nameof(ZoomOutDisplay),
        //    RoutingStrategy.Bubble,
        //    typeof(RoutedEventHandler),
        //    typeof(FrameControl));

        public static readonly RoutedEvent SimilarDisplayEvent = EventManager.RegisterRoutedEvent(
            nameof(SimilarDisplay),
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

        //public static readonly DependencyProperty ShowZoomOutProperty = DependencyProperty.Register(
        //    nameof(ShowZoomOut),
        //    typeof(bool),
        //    typeof(FrameControl),
        //    null);

        //public static readonly DependencyProperty ShowZoomIntoProperty = DependencyProperty.Register(
        //    nameof(ShowZoomInto),
        //    typeof(bool),
        //    typeof(FrameControl),
        //    null);

        public static readonly DependencyProperty IsClickedProperty = DependencyProperty.Register(
            nameof(IsClicked),
            typeof(bool),
            typeof(FrameControl),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsScrollingRightProperty = DependencyProperty.Register(
            nameof(IsScrollingRight),
            typeof(bool),
            typeof(FrameControl),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsScrollingLeftProperty = DependencyProperty.Register(
            nameof(IsScrollingLeft),
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

        //public bool ShowZoomOut
        //{
        //    get => (bool)GetValue(ShowZoomOutProperty);
        //    set => SetValue(ShowZoomOutProperty, value);
        //}

        //public bool ShowZoomInto
        //{
        //    get => (bool)GetValue(ShowZoomIntoProperty);
        //    set => SetValue(ShowZoomIntoProperty, value);
        //}

        public bool IsSelectable
        {
            get => (bool)GetValue(IsSelectableProperty);
            set => SetValue(IsSelectableProperty, value);
        }

        public bool IsNotSelectable => !IsSelectable;

        public bool IsClicked
        {
            get => (bool)GetValue(IsClickedProperty);
            set => SetValue(IsClickedProperty, value);
        }

        public bool IsScrollingRight
        {
            get => (bool)GetValue(IsScrollingRightProperty);
            set => SetValue(IsScrollingRightProperty, value);
        }

        public bool IsScrollingLeft
        {
            get => (bool)GetValue(IsScrollingLeftProperty);
            set => SetValue(IsScrollingLeftProperty, value);
        }

        public event RoutedEventHandler AddToQueryClicked
        {
            add => AddHandler(AddToQueryClickedEvent, value);
            remove => RemoveHandler(AddToQueryClickedEvent, value);
        }

        public event RoutedEventHandler AddToGpsQueryClicked
        {
            add => AddHandler(AddToGpsQueryClickedEvent, value);
            remove => RemoveHandler(AddToGpsQueryClickedEvent, value);
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

        //public event RoutedEventHandler SortDisplay
        //{
        //    add => AddHandler(SortDisplayEvent, value);
        //    remove => RemoveHandler(SortDisplayEvent, value);
        //}

        //public event RoutedEventHandler ZoomIntoDisplay
        //{
        //    add => AddHandler(ZoomIntoDisplayEvent, value);
        //    remove => RemoveHandler(ZoomIntoDisplayEvent, value);
        //}

        //public event RoutedEventHandler ZoomOutDisplay
        //{
        //    add => AddHandler(ZoomOutDisplayEvent, value);
        //    remove => RemoveHandler(ZoomOutDisplayEvent, value);
        //}
        
        public event RoutedEventHandler SimilarDisplay
        {
            add => AddHandler(SimilarDisplayEvent, value);
            remove => RemoveHandler(SimilarDisplayEvent, value);
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
                    //disabled for now
                    //IsClicked = true;
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
            IsScrollingLeft = false;
            IsScrollingRight = false;

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

            RaiseEvent(new RoutedEventArgs(ScrollVideoDisplayEvent));

            IsClicked = true;
            IsScrollingLeft = false;
            IsScrollingRight = false;

            if (e.Delta < 0)
            {
                IsScrollingRight = true;
                frameViewModel.ScrollToNextFrame();
            }
            else if (e.Delta > 0)
            {
                IsScrollingLeft = true;
                frameViewModel.ScrollToPreviousFrame();
            }

            e.Handled = true;
        }

        private void ButtonAddClicked(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Tag.ToString())
            {
                case "1":
                    RaiseEvent(new AddToQueryEventArgs(AddToQueryClickedEvent, true));
                    break;
                case "2":
                    RaiseEvent(new AddToQueryEventArgs(AddToQueryClickedEvent, false));
                    break;
                    default:
                    throw new ArgumentException("Unknown sender");
            }
        }
        //private void ButtonAddGpsClicked(object sender, RoutedEventArgs e)
        //{
        //    RaiseEvent(new RoutedEventArgs(AddToGpsQueryClickedEvent));
        //}

        private void ButtonRemovedClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RemoveFromQueryClickedEvent));
        }

        private void SubmitClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(FramesSubmittedEvent));
        }

        //private void SortedClicked(object sender, RoutedEventArgs e)
        //{
        //    RaiseEvent(new RoutedEventArgs(SortDisplayEvent));
        //}

        //private void ZoomIntoClicked(object sender, RoutedEventArgs e)
        //{
        //    RaiseEvent(new RoutedEventArgs(ZoomIntoDisplayEvent));
        //}
        //private void ZoomOutClicked(object sender, RoutedEventArgs e)
        //{
        //    RaiseEvent(new RoutedEventArgs(ZoomOutDisplayEvent));
        //}

        private void SimilarClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(SimilarDisplayEvent));
        }
        private void VideoClicked(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(VideoDisplayEvent));
        }
    }

    public class AddToQueryEventArgs : RoutedEventArgs
    {
        public AddToQueryEventArgs(RoutedEvent routedEvent, bool first) : base(routedEvent)
        {
            First = first;
        }

        public bool First { get; }
    }
}
