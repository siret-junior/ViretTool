using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.Core;
using ViretTool.PresentationLayer.Controls.Common;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    /// <summary>
    /// Base class for grid based display.
    /// 
    /// * The display viewport of size (DisplayWidth x DisplayHeight)
    ///   shows a subset VisibleFrames of size (ImageWidth x ImageHeight) from a superset of _loadedFrames
    ///   in a (ColumnCount x RowCount) grid.
    /// * Provides modal overlay for situations when background computation IsBusy.
    /// * Logs user interactions.
    /// * Provides an initial display that is loaded initially or additionally on an external request.
    /// * Holds a 1D list of loaded frames that needs to be displayed in 2D grid correctly
    ///   based on ColumnCount and RowCount. 
    /// * Updates correctly on changes of display viewport size.
    /// </summary>
    public abstract class DisplayControlViewModelBase : PropertyChangedBase
    {
        protected readonly IDatasetServicesManager _datasetServicesManager;
        protected readonly IInteractionLogger _interactionLogger;
        protected readonly ILogger _logger;
        
        protected List<FrameViewModel> _loadedFrames = new List<FrameViewModel>();
        protected int _defaultImageHeight;
        protected int _defaultImageWidth;

        private bool _isBusy;
        private int _rowCount;
        private int _columnCount;
        private int _imageHeight = 1;
        private int _imageWidth = 1;
        private bool _isInitialDisplayShown;


        protected DisplayControlViewModelBase(
            ILogger logger, 
            IDatasetServicesManager datasetServicesManager, 
            IInteractionLogger interactionLogger)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            _interactionLogger = interactionLogger;
            _datasetServicesManager.DatasetOpened += 
                (_, services) =>
                {
                    ImageHeight = _defaultImageHeight = services.DatasetParameters.DefaultFrameHeight;
                    ImageWidth = _defaultImageWidth = services.DatasetParameters.DefaultFrameWidth;
                };
        }


        #region --[ Properties ]--

        public BindableCollection<FrameViewModel> VisibleFrames { get; } = new BindableCollection<FrameViewModel>();

        public int DisplayHeight { get; set; }

        public int DisplayWidth { get; set; }

        public int ImageHeight
        {
            get => _imageHeight;
            set
            {
                if (_imageHeight == value)
                {
                    return;
                }

                _imageHeight = value;
                NotifyOfPropertyChange();
            }
        }

        public int ImageWidth
        {
            get => _imageWidth;
            set
            {
                if (_imageWidth == value)
                {
                    return;
                }

                _imageWidth = value;
                NotifyOfPropertyChange();
            }
        }

        public int ColumnCount
        {
            get => _columnCount;
            set
            {
                if (_columnCount == value)
                {
                    return;
                }

                _columnCount = value;
                NotifyOfPropertyChange();
            }
        }

        public int RowCount
        {
            get => _rowCount;
            set
            {
                if (_rowCount == value)
                {
                    return;
                }

                _rowCount = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                {
                    return;
                }

                _isBusy = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsInitialDisplayShown
        {
            get => _isInitialDisplayShown;
            set
            {
                if (_isInitialDisplayShown == value)
                {
                    return;
                }

                _isInitialDisplayShown = value;
                NotifyOfPropertyChange();
            }
        }

        public double LargeFramesMultiplier => 1.5;

        public virtual bool IsLargeFramesChecked { get; set; } = false;

        #endregion --[ Properties ]--


        #region --[ Actions ]--

        public Action<int> ScrollToRow { protected get; set; }

        public Action ResetGrid { protected get; set; }

        // TODO: check usage
        public Action DisplaySizeChangedHandler =>
            () =>
            {
                ResetGrid?.Invoke();
                UpdateVisibleFrames();
            };

        #endregion --[ Actions ]--


        #region --[ Event handlers ]--

        public event EventHandler<FramesToQuery> FramesForQueryChanged;             // similarity query
        public event EventHandler<FrameViewModel> FrameForVideoChanged;             // video inspection
        public event EventHandler<FrameViewModel> FrameForScrollVideoChanged;       // TODO: rename. (video scrolling playback / scrolling sidebar?)
        public event EventHandler<FrameViewModel> FrameForSortChanged;              // TODO: remove? (unused - used to be noodle map)
        public event EventHandler<FrameViewModel> FrameForGpsChanged;               // lifelog specific GPS coordinates
        public event EventHandler<FrameViewModel> FrameForZoomIntoChanged;          // ZoomDisplay zoom in
        public event EventHandler<FrameViewModel> FrameForZoomOutChanged;           // ZoomDisplay zoom out
        public event EventHandler<IList<FrameViewModel>> SubmittedFramesChanged;    // submit collection

        #endregion --[ Event handlers ]--


        #region --[ Events ]--

        public ISubject<Unit> QuerySettingsChanged { get; } = new Subject<Unit>();

        public void OnAddToQueryClicked(FrameViewModel frameViewModel, AddToQueryEventArgs args)
        {
            frameViewModel.IsSelectedForQuery = true;
            BeforeEventAction();
            bool supressQueryChange = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            FramesForQueryChanged?.Invoke(
                this,
                new FramesToQuery(_loadedFrames.Where(f => f.IsSelectedForQuery).Append(frameViewModel).Distinct().ToList(), args.First, supressQueryChange));

            if (supressQueryChange)
            {
                _loadedFrames.ForEach(f => f.IsSelectedForQuery = false);
            }
        }

        public void OnAddToGpsClicked(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            FrameForGpsChanged?.Invoke(this, frameViewModel);
        }

        public void OnFrameSelected(FrameViewModel frameViewModel)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                frameViewModel.IsSelectedForQuery = !frameViewModel.IsSelectedForQuery;
            }
            else if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                _loadedFrames.ForEach(f => f.IsSelectedForQuery = false);
            }
            
        }

        public void OnFramesSubmitted(FrameViewModel frameViewModel)
        {
            SubmittedFramesChanged?.Invoke(
                this,
                _loadedFrames.Where(f => f.IsSelectedForQuery)
                             .Append(frameViewModel)
                             .Distinct()
                             .Select(f => new FrameViewModel(f.VideoId, f.FrameNumber, _datasetServicesManager))
                             .ToList());
        }

        // TODO: remove, unused?
        public void OnSortDisplay(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            FrameForSortChanged?.Invoke(this, frameViewModel);
        }

        public void OnZoomIntoDisplay(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            FrameForZoomIntoChanged?.Invoke(this, frameViewModel);
        }

        public void OnZoomOutDisplay(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            FrameForZoomOutChanged?.Invoke(this, frameViewModel);
        }

        public void OnVideoDisplay(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            FrameForVideoChanged?.Invoke(this, frameViewModel);
        }

        public void OnScrollVideoDisplay(FrameViewModel frameViewModel)
        {
            FrameForScrollVideoChanged?.Invoke(this, frameViewModel.Clone());
        }

        public void OnGridScrollChanged(LogType logType, string detailDescription = null)
        {
            _interactionLogger.LogInteraction(LogCategory.Browsing, logType, "ScrollChanged", detailDescription);
        }

        #endregion --[ Events ]--


        #region --[ Public methods ]--

        public int[] GetTopFrameIds(int count) => _loadedFrames
            .Select(GetFrameId)
            .Where(id => id.HasValue)
            .Take(count)
            .Cast<int>()
            .ToArray();

        /// <summary>
        /// Loads all parent video frames of the input frame.
        /// </summary>
        /// <param name="frameViewModel">input frame</param>
        /// <returns>async Task</returns>
        public virtual async Task LoadVideoForFrame(FrameViewModel frameViewModel)
        {
            int videoId = frameViewModel.VideoId;
            _loadedFrames = await Task.Run(
                () => _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumbersForVideo(videoId)
                            .Select(frameId => ConvertThumbnailToViewModel(videoId, frameId))
                            .ToList());
            SelectFrame(frameViewModel);
            IsInitialDisplayShown = false;
            UpdateVisibleFrames();
        }

        /// <summary>
        /// Loads FrameViewModel frame wrappers for the input set if frameIds.
        /// </summary>
        /// <param name="sortedFrameIds"></param>
        /// <returns></returns>
        public virtual async Task LoadFramesForIds(IEnumerable<int> sortedFrameIds)
        {
            // TODO: .Where(f => f != null) should not happen! Investigate why is this used here and possibly remove it!
            _loadedFrames = await Task.Run(() => sortedFrameIds.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());
            IsInitialDisplayShown = false;
            UpdateVisibleFrames();
        }

        // TODO: move this implementation to PageDisplay, leave an abstract method.
        public virtual async Task LoadInitialDisplay()
        {
            IsInitialDisplayShown = false;
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            if (_datasetServicesManager.CurrentDataset.DatasetParameters.IsInitialDisplayPrecomputed)
            {
                IReadOnlyList<int> ids = _datasetServicesManager.CurrentDataset.InitialDisplayProvider.InitialDisplayIds;
                _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());
                AddFramesToVisibleItems(VisibleFrames, _loadedFrames);

                RowCount = _datasetServicesManager.CurrentDataset.InitialDisplayProvider.RowCount;
                ColumnCount = _datasetServicesManager.CurrentDataset.InitialDisplayProvider.ColumnCount;
                ScrollToRow(0);
                IsInitialDisplayShown = true;
            }
            else
            {
                Random random = new Random(); //shuffle initial images randomly
                _loadedFrames = await Task.Run(() => datasetService.VideoIds.SelectMany(LoadAllThumbnails).OrderBy(_ => random.Next()).ToList());
                UpdateVisibleFrames();
            }
        }

        #endregion --[ Public methods ]--


        protected void NotifyQuerySettingsChanged()
        {
            QuerySettingsChanged.OnNext(Unit.Default);
        }

        protected virtual void BeforeEventAction()
        {
        }

        // TODO: remove/rename?
        protected virtual void UpdateVisibleFrames()
        {
            AddFramesToVisibleItems(VisibleFrames, _loadedFrames);
        }

        protected virtual void AddFramesToVisibleItems(
            BindableCollection<FrameViewModel> collectionToUpdate, 
            IList<FrameViewModel> viewModelsToAdd)
        {
            viewModelsToAdd.ForEach(vm => vm.IsVisible = true);

            int i = 0;
            for (; i < collectionToUpdate.Count && i < viewModelsToAdd.Count; i++)
            {
                collectionToUpdate[i] = viewModelsToAdd[i];
                collectionToUpdate[i].IsVisible = true;
            }

            if (collectionToUpdate.Count < viewModelsToAdd.Count)
            {
                collectionToUpdate.AddRange(viewModelsToAdd.Skip(collectionToUpdate.Count).ToList());
            }
            else if (viewModelsToAdd.Count < collectionToUpdate.Count)
            {
                for (; i < collectionToUpdate.Count; i++)
                {
                    collectionToUpdate[i].IsVisible = viewModelsToAdd.Contains(collectionToUpdate[i]);
                }
            }
        }

        protected FrameViewModel SelectFrame(FrameViewModel frame)
        {
            _loadedFrames.ForEach(f => f.IsSelectedForDetail = false);
            FrameViewModel selectedFrame = _loadedFrames.FirstOrDefault(f => f.VideoId == frame.VideoId && f.FrameNumber == frame.FrameNumber);
            if (selectedFrame != null)
            {
                selectedFrame.IsSelectedForDetail = true;
            }

            return selectedFrame;
        }

        // TODO: use a pool of FrameViewModels to reduce memory allocation
        protected FrameViewModel ConvertThumbnailToViewModel(int videoId, int frameNumber)
        {
            return new FrameViewModel(videoId, frameNumber, _datasetServicesManager);
        }

        protected int? GetFrameId(FrameViewModel frame)
        {
            return !_datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(frame.VideoId, frame.FrameNumber, out int frameId) ? (int?)null : frameId;
        }
        
        protected FrameViewModel GetFrameViewModelForFrameId(int frameId)
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            int videoId = datasetService.GetVideoIdForFrameId(frameId);
            int frameNumber = datasetService.GetFrameNumberForFrameId(frameId);
            return ConvertThumbnailToViewModel(videoId, frameNumber);
        }

        private IEnumerable<FrameViewModel> LoadAllThumbnails(int videoId)
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            return datasetService.GetFrameNumbersForVideo(videoId).Select(frameNumber => ConvertThumbnailToViewModel(videoId, frameNumber));
        }
    }
}
