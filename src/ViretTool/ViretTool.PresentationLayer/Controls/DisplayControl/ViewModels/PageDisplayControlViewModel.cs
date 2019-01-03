using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class PageDisplayControlViewModel : DisplayControlViewModelBase
    {
        private int _currentPageNumber;

        private bool _isMax1FromShotChecked;
        private bool _isMax3FromVideoChecked;

        public PageDisplayControlViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager)
            : base(logger, datasetServicesManager)
        {
        }

        public bool IsMax1FromShotChecked
        {
            get => _isMax1FromShotChecked;
            set
            {
                if (_isMax1FromShotChecked == value)
                {
                    return;
                }

                _isMax1FromShotChecked = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsMax3FromVideoChecked
        {
            get => _isMax3FromVideoChecked;
            set
            {
                if (_isMax3FromVideoChecked == value)
                {
                    return;
                }

                _isMax3FromVideoChecked = value;
                NotifyOfPropertyChange();
            }
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

        public int LastPageNumber => _loadedFrames.Any() ? (int)Math.Ceiling(_loadedFrames.Count / (double)(DisplayHeight / ImageHeight * (DisplayWidth / ImageWidth))) - 1 : 0;

        public void FirstPageButton()
        {
            CurrentPageNumber = 0;
            UpdateVisibleFrames();
        }

        public void LastPageButton()
        {
            CurrentPageNumber = LastPageNumber;
            UpdateVisibleFrames();
        }

        public void NextPageButton()
        {
            if (CurrentPageNumber >= LastPageNumber)
            {
                return;
            }

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
            NotifyOfPropertyChange(nameof(LastPageNumber));
            int itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            List<FrameViewModel> viewModelsToAdd = _loadedFrames.Skip(CurrentPageNumber * itemsCount).Take(itemsCount).ToList();

            AddFramesToVisibleItems(VisibleFrames, viewModelsToAdd);
        }
    }
}
