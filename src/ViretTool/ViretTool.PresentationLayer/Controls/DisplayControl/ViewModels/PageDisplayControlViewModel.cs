using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.SubmitControl.ViewModels;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class PageDisplayControlViewModel : DisplayControlViewModelBase
    {
        private int _currentPageNumber;

        public PageDisplayControlViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager, IWindowManager windowManager, SubmitControlViewModel submitControlViewModel)
            : base(logger, datasetServicesManager, windowManager, submitControlViewModel)
        {
        }

        public int CurrentPageNumber
        {
            get => _currentPageNumber;
            set
            {
                if (_currentPageNumber == value)
                {
                    return;
                }

                _currentPageNumber = value;
                NotifyOfPropertyChange();
            }
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

        public override Task LoadInitialDisplay()
        {
            CurrentPageNumber = 0;
            return base.LoadInitialDisplay();
        }

        public override Task LoadFramesForIds(IEnumerable<int> sortedFrameIds)
        {
            CurrentPageNumber = 0;
            return base.LoadFramesForIds(sortedFrameIds);
        }

        public override Task LoadVideoForFrame(FrameViewModel frameViewModel)
        {
            CurrentPageNumber = 0;
            return base.LoadVideoForFrame(frameViewModel);
        }

        protected override void UpdateVisibleFrames()
        {
            int itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            List<FrameViewModel> viewModelsToAdd = _loadedFrames.Skip(CurrentPageNumber * itemsCount).Take(itemsCount).ToList();
            if (VisibleFrames.Count != itemsCount || VisibleFrames.Count != viewModelsToAdd.Count)
            {
                VisibleFrames.Clear();
                VisibleFrames.AddRange(viewModelsToAdd);
                return;
            }

            int i = 0;
            foreach (FrameViewModel frameViewModel in viewModelsToAdd)
            {
                VisibleFrames[i++] = frameViewModel;
            }
        }
    }
}
