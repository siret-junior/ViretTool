using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class ResultDisplayViewModel : DisplayControlViewModelBase
    {
        private int _currentPageNumber;
        private int _maxFramesFromShot;
        private int _maxFramesFromVideo;

        public ResultDisplayViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger iterationLogger)
            : base(logger, datasetServicesManager, iterationLogger)
        {
            datasetServicesManager.DatasetOpened += 
                (_, services) =>
                {
                    // TODO: pull these parameters from properties
                    _maxFramesFromVideo = services.DatasetParameters.IsLifelogData ? 50 : 3;
                    _maxFramesFromShot = services.DatasetParameters.IsLifelogData ? 5 : 1;
                    NotifyOfPropertyChange(nameof(MaxFramesFromVideo));
                    NotifyOfPropertyChange(nameof(MaxFramesFromShot));
                };
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
                _interactionLogger.LogInteraction(LogCategory.Filter, LogType.MaxFrames, $"{MaxFramesFromVideo}|{MaxFramesFromShot}");
                NotifyQuerySettingsChanged();
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
                _interactionLogger.LogInteraction(LogCategory.Filter, LogType.MaxFrames, $"{MaxFramesFromVideo}|{MaxFramesFromShot}");
                NotifyQuerySettingsChanged();
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
                _interactionLogger.LogInteraction(LogCategory.Browsing, LogType.RankedList, $"CurrentPage:{value}");
                NotifyOfPropertyChange();
            }
        }

        public int LastPageNumber => _loadedFrames.Any() ? (int)Math.Ceiling(_loadedFrames.Count / ((double)RowCount * ColumnCount)) - 1 : 0;

        
        public void FirstPageButton()
        {
            if (CurrentPageNumber == 0)
            {
                return;
            }

            CurrentPageNumber = 0;
            UpdateVisibleFrames();
        }

        public void LastPageButton()
        {
            if (CurrentPageNumber == LastPageNumber)
            {
                return;
            }

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


        public override async Task LoadInitialDisplay()
        {
            CurrentPageNumber = 0;
            await base.LoadInitialDisplay();
            NotifyOfPropertyChange(nameof(LastPageNumber));
        }

        public override async Task LoadFramesForIds(IList<int> sortedFrameIds)
        {
            CurrentPageNumber = 0;
            await base.LoadFramesForIds(sortedFrameIds);
            for (int i = 0; i < _loadedFrames.Count() - ColumnCount; i++)
            {
                _loadedFrames[i].IsBottomBorderVisible = false;
                _loadedFrames[i].IsRightBorderVisible = false;
            }
        }

        public override Task LoadVideoForFrame(FrameViewModel frameViewModel)
        {
            CurrentPageNumber = 0;
            return base.LoadVideoForFrame(frameViewModel);
        }

        protected override void UpdateVisibleFrames()
        {
            // don't update if there is nothing to update
            if (_loadedFrames.Count == 0) return;

            // update rows and columns based on display size
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;
            
            List<FrameViewModel> viewModelsToAdd = _loadedFrames;

            ScrollToRow?.Invoke(0);
            VisibleFrames.Clear();
            viewModelsToAdd.ForEach(f => f.IsLastInVideo = false);

            // show temporal context
            List<FrameViewModel> viewModelsWithContext = new List<FrameViewModel>();
            int middleFrameOffset = ColumnCount / 2;
            for (int iFrame = 0; iFrame < viewModelsToAdd.Count && iFrame < 200; iFrame++)
            {
                FrameViewModel primaryViewModel = viewModelsToAdd[iFrame];
                int[] videoFrameNumbers = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumbersForVideo(primaryViewModel.VideoId);
                int frameIndex = Array.IndexOf(videoFrameNumbers, primaryViewModel.FrameNumber);
                int startIndex = (frameIndex - middleFrameOffset >= 0) ? frameIndex - middleFrameOffset : 0;


                List<FrameViewModel> frameContext = videoFrameNumbers
                    .Skip(startIndex)
                    .Take(ColumnCount)
                    .Select(frameNumber => ConvertThumbnailToViewModel(primaryViewModel.VideoId, frameNumber))
                    .ToList();

                while (frameContext.Count() < ColumnCount)
                {
                    frameContext.Add(ConvertThumbnailToViewModel(0, 0));
                }

                viewModelsWithContext.AddRange(frameContext);
            }
            viewModelsToAdd = viewModelsWithContext;

            AddFramesToVisibleItems(VisibleFrames, viewModelsToAdd);
        }


    }
}
