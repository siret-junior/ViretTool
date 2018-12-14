using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.SubmitControl.ViewModels
{
    public class SubmitControlViewModel : Screen
    {
        public SubmitControlViewModel()
        {
            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight) * 2;
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth) * 2;
        }

        public int ImageHeight { get; set; }

        public int ImageWidth { get; }

        public BindableCollection<FrameViewModel> SubmittedFrames { get; } = new BindableCollection<FrameViewModel>();

        public void Initialize(IList<FrameViewModel> selectedFrames)
        {
            SubmittedFrames.Clear();
            SubmittedFrames.AddRange(selectedFrames);
        }

        public void RemoveFromQueryClicked(FrameViewModel frameViewModel)
        {
            SubmittedFrames.Remove(frameViewModel);
        }

        public void Ok()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
