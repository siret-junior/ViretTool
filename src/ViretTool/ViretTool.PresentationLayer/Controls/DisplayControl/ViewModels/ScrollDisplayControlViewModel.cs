using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class ScrollDisplayControlViewModel : ScrollableDisplayControlViewModel
    {
        private FrameViewModel _lastSelectedFrame;

        public ScrollDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger interactionLogger)
            : base(logger, datasetServicesManager, interactionLogger)
        {
        }

        public override async Task LoadVideoForFrame(FrameViewModel selectedFrame)
        {
            if (_lastSelectedFrame?.VideoId != selectedFrame.VideoId)
            {
                _lastSelectedFrame = selectedFrame;
                await base.LoadVideoForFrame(selectedFrame);
            }

            FrameViewModel newlySelectedFrame = SelectFrame(selectedFrame);
            if (newlySelectedFrame != null)
            {
                _lastSelectedFrame = newlySelectedFrame;
                ScrollToFrameHorizontally(newlySelectedFrame);
            }
            else
            {
                _lastSelectedFrame.IsSelectedForDetail = true;
            }
        }

        protected override void UpdateVisibleFrames()
        {
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;

            base.UpdateVisibleFrames();
            //VisibleFrames.RemoveRange(VisibleFrames.Where(f => !f.IsVisible).ToList());
        }
    }
}
