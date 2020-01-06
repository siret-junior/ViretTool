using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class PageDisplayControlViewModel : DisplayControlViewModelBase
    {
        private int _currentPageNumber;
        private int _maxFramesFromShot = 1;
        private int _maxFramesFromVideo = 3;
        private bool _isLargeFramesChecked;
        private FrameViewModel _gpsFrame;

        public PageDisplayControlViewModel(
            ILogger logger,
            IDatasetServicesManager datasetServicesManager,
            IInteractionLogger iterationLogger)
            : base(logger, datasetServicesManager, iterationLogger)
        {
            datasetServicesManager.DatasetOpened += (_, services) =>
                                                    {
                                                        _maxFramesFromVideo = services.DatasetParameters.IsLifelogData ? 50 : 3;
                                                        NotifyOfPropertyChange(nameof(MaxFramesFromVideo));
                                                    };
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
                
                // TODO: enable lifelog filter
                //_interactionLogger.LogInteraction(LogCategory.Filter, LogType.Lifelog, _gpsFrame == null ? "" : $"{_gpsFrame.VideoId}|{_gpsFrame.FrameNumber}");
                NotifyQuerySettingsChanged();
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

        public bool IsLargeFramesChecked
        {
            get => _isLargeFramesChecked;
            set
            {
                if (_isLargeFramesChecked == value)
                {
                    return;
                }

                _isLargeFramesChecked = value;
                OnLargeFramesChanged();
                NotifyOfPropertyChange();
            }
        }

        public ISubject<Unit> QuerySettingsChanged { get; } = new Subject<Unit>();

        public double LargeFramesMultiplier => 1.5;

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

        public void DeleteGpsFrame()
        {
            GpsFrame = null;
        }

        public override async Task LoadInitialDisplay()
        {
            CurrentPageNumber = 0;
            await base.LoadInitialDisplay();
            NotifyOfPropertyChange(nameof(LastPageNumber));
        }

        public override async Task LoadFramesForIds(IEnumerable<int> sortedFrameIds)
        {
            CurrentPageNumber = 0;
            await base.LoadFramesForIds(sortedFrameIds);
        }

        public override Task LoadVideoForFrame(FrameViewModel frameViewModel)
        {
            CurrentPageNumber = 0;
            return base.LoadVideoForFrame(frameViewModel);
        }

        protected override void UpdateVisibleFrames()
        {
            RowCount = DisplayHeight / ImageHeight;
            ColumnCount = DisplayWidth / ImageWidth;
            NotifyOfPropertyChange(nameof(LastPageNumber));
            List<FrameViewModel> viewModelsToAdd = _loadedFrames;
            if (!IsLargeFramesChecked)
            {
                int itemsCount = RowCount * ColumnCount;
                //order by top fram in a video ID a then by frame numbers in a video
                viewModelsToAdd = _loadedFrames.Skip(CurrentPageNumber * itemsCount)
                                               .Take(itemsCount)
                                               .GroupBy(f => f.VideoId)
                                               .SelectMany(
                                                   g =>
                                                   {
                                                       FrameViewModel[] orderedFrames = g.OrderBy(f => f.FrameNumber).ToArray();
                                                       orderedFrames[orderedFrames.Length - 1].IsLastInVideo = true;
                                                       return orderedFrames;
                                                   })
                                               .ToList();
            }
            else
            {
                ScrollToRow(0);
                VisibleFrames.Clear();
                viewModelsToAdd.ForEach(f => f.IsLastInVideo = false);

                // show temporal context
                List<FrameViewModel> viewModelsWithContext = new List<FrameViewModel>();
                for (int iFrame = 0; iFrame < viewModelsToAdd.Count && iFrame < 200; iFrame++)
                {
                    FrameViewModel primaryViewModel = viewModelsToAdd[iFrame];
                    int[] videoFrameNumbers = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumbersForVideo(primaryViewModel.VideoId);
                    int frameIndex = Array.IndexOf(videoFrameNumbers, primaryViewModel.FrameNumber);
                    int startIndex = (frameIndex - 2 >= 0) ? frameIndex - 2 : 0;


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
            }

            AddFramesToVisibleItems(VisibleFrames, viewModelsToAdd);
        }

        private void NotifyQuerySettingsChanged()
        {
            QuerySettingsChanged.OnNext(Unit.Default);
        }

        private void OnLargeFramesChanged()
        {
            if (_isLargeFramesChecked)
            {
                ImageHeight = (int)(_defaultImageHeight * LargeFramesMultiplier);
                ImageWidth = (int)(_defaultImageWidth * LargeFramesMultiplier);
            }
            else
            {
                ImageHeight = _defaultImageHeight;
                ImageWidth = _defaultImageWidth;
            }

            ScrollToRow(0);
            ResetGrid();
            VisibleFrames.Clear();
            UpdateVisibleFrames();
        }
    }
}
