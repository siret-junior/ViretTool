using System;
using System.Linq;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class PageDisplayControlViewModel : DisplayControlViewModelBase
    {
        public PageDisplayControlViewModel(IDatasetServicesManager datasetServicesManager) : base(datasetServicesManager)
        {
        }

        public void FirstPageButton()
        {
            CurrentPageNumber = 0;
            UpdateVisibleFrames();
        }

        public void LastPageButton()
        {
            var itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            CurrentPageNumber = (int)Math.Ceiling(_loadedFrames.Count / (double)itemsCount) - 1;
            UpdateVisibleFrames();
        }

        public void NextPageButton()
        {
            CurrentPageNumber++;
            UpdateVisibleFrames();
        }

        public void PreviousPageButton()
        {
            if (CurrentPageNumber <= 0)
            {
                return;
            }

            CurrentPageNumber--;
            UpdateVisibleFrames();
        }

        protected override void UpdateVisibleFrames()
        {
            int itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            VisibleFrames.Clear();
            VisibleFrames.AddRange(_loadedFrames.Skip(CurrentPageNumber * itemsCount).Take(itemsCount));
        }
    }
}
