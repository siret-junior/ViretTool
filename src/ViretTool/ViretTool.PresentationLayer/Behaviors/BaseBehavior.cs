using System.Windows;
using System.Windows.Interactivity;

namespace ViretTool.PresentationLayer.Behaviors
{
    public abstract class BaseBehavior<T> : Behavior<T> where T : FrameworkElement
    {
        private bool _isAttached;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        protected override void OnDetaching()
        {
            InvokeCleanUp();
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            base.OnDetaching();
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!_isAttached)
            {
                WireUp();
                _isAttached = true;
            }
        }

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InvokeCleanUp();
        }

        private void InvokeCleanUp()
        {
            if (_isAttached)
            {
                CleanUp();
                _isAttached = false;
            }
        }

        /// <summary>
        /// Connect events + other stuff
        /// </summary>
        protected abstract void WireUp();

        /// <summary>
        /// Disconnect events + other stuff
        /// </summary>
        protected abstract void CleanUp();
    }
}
