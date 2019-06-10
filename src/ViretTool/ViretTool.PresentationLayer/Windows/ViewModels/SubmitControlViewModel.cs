using System.Collections.Generic;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class SubmitControlViewModel : Screen
    {
        private int _imageHeight;
        private int _imageWidth;

        public SubmitControlViewModel(IDatasetServicesManager datasetServicesManager)
        {
            datasetServicesManager.DatasetOpened += (_, services) =>
                                                    {
                                                        ImageHeight = services.DatasetParameters.DefaultFrameHeight;
                                                        ImageWidth = services.DatasetParameters.DefaultFrameWidth;
                                                    };
        }

        public int ImageHeight
        {
            get => _imageHeight;
            private set
            {
                _imageHeight = value;
                NotifyOfPropertyChange();
            }
        }

        public int ImageWidth
        {
            get => _imageWidth;
            private set
            {
                _imageWidth = value;
                NotifyOfPropertyChange();
            }
        }

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
