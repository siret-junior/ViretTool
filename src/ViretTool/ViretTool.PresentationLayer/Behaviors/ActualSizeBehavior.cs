using System;
using System.Reactive.Linq;
using System.Windows;

namespace ViretTool.PresentationLayer.Behaviors
{
    public class ActualSizeBehavior : BaseBehavior<FrameworkElement>
    {
        public static readonly DependencyProperty ActualWidthProperty = DependencyProperty.Register(
            nameof(ActualWidth),
            typeof(int),
            typeof(ActualSizeBehavior),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ActualHeightProperty = DependencyProperty.Register(
            nameof(ActualHeight),
            typeof(int),
            typeof(ActualSizeBehavior),
            new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SizeChangedProperty = DependencyProperty.Register(
            nameof(SizeChanged),
            typeof(Action),
            typeof(ActualSizeBehavior),
            new FrameworkPropertyMetadata(default(Action), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private IDisposable _subscriber;

        public int ActualWidth
        {
            get => (int)GetValue(ActualWidthProperty);
            set => SetValue(ActualWidthProperty, value);
        }

        public int ActualHeight
        {
            get => (int)GetValue(ActualHeightProperty);
            set => SetValue(ActualHeightProperty, value);
        }

        public Action SizeChanged
        {
            get => (Action)GetValue(SizeChangedProperty);
            set => SetValue(SizeChangedProperty, value);
        }

        protected override void WireUp()
        {
            _subscriber = Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(
                                        h => AssociatedObject.SizeChanged += h,
                                        h => AssociatedObject.SizeChanged -= h).Throttle(TimeSpan.FromMilliseconds(700))
                                    .ObserveOn(AssociatedObject.Dispatcher)
                                    .Subscribe(_ => AssociatedObjectOnSizeChanged());
            AssociatedObjectOnSizeChanged();
        }

        private void AssociatedObjectOnSizeChanged()
        {
            ActualHeight = (int)AssociatedObject.ActualHeight;
            ActualWidth = (int)AssociatedObject.ActualWidth;
            SizeChanged?.Invoke();
        }

        protected override void CleanUp()
        {
            _subscriber?.Dispose();
        }
    }
}
