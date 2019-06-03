using System;
using System.Windows;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Behaviors
{
    public class ResetVirtualizedCacheBehavior : BaseBehavior<VirtualizedUniformGrid>
    {
        public static readonly DependencyProperty ResetProperty = DependencyProperty.Register(
            nameof(Reset),
            typeof(Action),
            typeof(ResetVirtualizedCacheBehavior),
            null);

        public Action Reset
        {
            get => (Action)GetValue(ResetProperty);
            set => SetValue(ResetProperty, value);
        }

        protected override void WireUp()
        {
            Reset = AssociatedObject.ResetCache;
        }

        protected override void CleanUp()
        {
            
        }
    }
}
