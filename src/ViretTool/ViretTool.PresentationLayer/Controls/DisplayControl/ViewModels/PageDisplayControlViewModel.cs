using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class PageDisplayControlViewModel : DisplayControlViewModelBase
    {
        private readonly IInteractionLogger _iterationLogger;
        private int _currentPageNumber;

        private int _maxFramesFromShot = 1;
        private int _maxFramesFromVideo = 3;
        private FrameViewModel _gpsFrame;

        public PageDisplayControlViewModel(ILogger logger, IDatasetServicesManager datasetServicesManager, IInteractionLogger iterationLogger)
            : base(logger, datasetServicesManager)
        {
            _iterationLogger = iterationLogger;
        }

        public FrameViewModel GpsFrame
        {
            get => _gpsFrame;
            set {
                if (_gpsFrame?.Equals(value) == true)
                {
                    return;
                }

                _gpsFrame = value;
                NotifyOfPropertyChange();
            }
        }

        public int MaxFramesFromShot
        {
            get => _maxFramesFromShot;
            set
            {
                if (_maxFramesFromShot == value)
                {
                    return;
                }

                _maxFramesFromShot = value;
                _iterationLogger.LogInteraction(LogCategory.Filter, LogType.MaxFrames, $"{MaxFramesFromVideo}|{MaxFramesFromShot}");
                MaxFramesChanged?.Invoke(this, EventArgs.Empty);
                NotifyOfPropertyChange();
            }
        }

        public int MaxFramesFromVideo
        {
            get => _maxFramesFromVideo;
            set
            {
                if (_maxFramesFromVideo == value)
                {
                    return;
                }

                _maxFramesFromVideo = value;
                _iterationLogger.LogInteraction(LogCategory.Filter, LogType.MaxFrames, $"{MaxFramesFromVideo}|{MaxFramesFromShot}");
                MaxFramesChanged?.Invoke(this, EventArgs.Empty);
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
                _iterationLogger.LogInteraction(LogCategory.Browsing, LogType.RankedList, $"CurrentPage:{value}");
                NotifyOfPropertyChange();
            }
        }

        public int LastPageNumber => _loadedFrames.Any() ? (int)Math.Ceiling(_loadedFrames.Count / (double)(DisplayHeight / ImageHeight * (DisplayWidth / ImageWidth))) - 1 : 0;

        public event EventHandler MaxFramesChanged;

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
