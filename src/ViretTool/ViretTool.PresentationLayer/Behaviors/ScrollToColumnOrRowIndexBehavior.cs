using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Helpers;

namespace ViretTool.PresentationLayer.Behaviors
{
    public class ScrollToColumnOrRowIndexBehavior : BaseBehavior<ScrollViewer>
    {
        public static readonly DependencyProperty ScrollToColumnProperty = DependencyProperty.Register(
            nameof(ScrollToColumn),
            typeof(Action<int>),
            typeof(ScrollToColumnOrRowIndexBehavior),
            null);

        public static readonly DependencyProperty ScrollToRowProperty = DependencyProperty.Register(
            nameof(ScrollToRow),
            typeof(Action<int>),
            typeof(ScrollToColumnOrRowIndexBehavior),
            null);

        public Action<int> ScrollToColumn
        {
            get => (Action<int>)GetValue(ScrollToColumnProperty);
            set => SetValue(ScrollToColumnProperty, value);
        }

        public Action<int> ScrollToRow
        {
            get => (Action<int>)GetValue(ScrollToRowProperty);
            set => SetValue(ScrollToRowProperty, value);
        }

        protected override void CleanUp()
        {
        }


        protected override void WireUp()
        {
            ScrollToColumn = ScrollToColumnNumber;
            ScrollToRow = ScrollToRowNumber;
        }


        private async void ScrollToColumnNumber(int columnNumber)
        {
            FrameControl frameControl = await GetFrameControl();
            AssociatedObject.ScrollToHorizontalOffset(columnNumber * frameControl?.ActualWidth ?? 0);
        }

        private async void ScrollToRowNumber(int rowNumber)
        {
            FrameControl frameControl = await GetFrameControl();
            AssociatedObject.ScrollToVerticalOffset(rowNumber * frameControl?.ActualHeight ?? 0);
        }

        private async Task<FrameControl> GetFrameControl()
        {
            FrameControl frameControl = AssociatedObject.FindChild<FrameControl>();
            //a bit of hack, we have to wait till the FrameControls are drawn
            for (int i = 0; i < 5 && frameControl == null; i++)
            {
                await Task.Delay(100);
                frameControl = AssociatedObject.FindChild<FrameControl>();
            }

            return frameControl;
        }
    }
}
