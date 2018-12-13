using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Helpers;

namespace ViretTool.PresentationLayer.Behaviors
{
    public class ScrollToColumnIndexBehavior : BaseBehavior<ScrollViewer>
    {
        public static readonly DependencyProperty ScrollToColumnProperty = DependencyProperty.Register(
            nameof(ScrollToColumn),
            typeof(Action<int>),
            typeof(ScrollToColumnIndexBehavior),
            null);

        public Action<int> ScrollToColumn
        {
            get => (Action<int>)GetValue(ScrollToColumnProperty);
            set => SetValue(ScrollToColumnProperty, value);
        }

        protected override void CleanUp()
        {
        }


        protected override void WireUp()
        {
            ScrollToColumn = ScrollToColumnNumber;
        }

        

        private async void ScrollToColumnNumber(int columnNumber)
        {
            FrameControl frameControl = AssociatedObject.FindChild<FrameControl>();
            //a bit of hack, we have to wait till the FrameControls are drawn
            for (int i = 0; i < 5 && frameControl == null; i++)
            {
                await Task.Delay(100);
                frameControl = AssociatedObject.FindChild<FrameControl>();
            }

            AssociatedObject.ScrollToHorizontalOffset(columnNumber * frameControl?.ActualWidth ?? 0);
        }
    }
}
