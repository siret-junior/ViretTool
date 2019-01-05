using System.IO;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public class DownloadedFrameViewModel : FrameViewModel
    {
        public DownloadedFrameViewModel(IDatasetServicesManager servicesManager, string imagePath) : base(-1, -1, servicesManager)
        {
            ImagePath = imagePath;
            ImageSource = File.ReadAllBytes(imagePath);
        }

        public string ImagePath { get; }

        public override byte[] ImageSource { get; }
    }
}
