using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using Action = System.Action;
using EnumerableExtensions = ViretTool.Core.EnumerableExtensions;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
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
        private int _imageHeight;
        private int _imageWidth;
        

        protected DisplayControlViewModelBase(ILogger logger, IDatasetServicesManager datasetServicesManager, IInteractionLogger interactionLogger)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            _interactionLogger = interactionLogger;
            _imageHeight = _defaultImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight);
            _imageWidth = _defaultImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth);
        }

        public int DisplayHeight { get; set; }

        public int DisplayWidth { get; set; }

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

        public Action DisplaySizeChangedHandler => () =>
                                                   {
                                                       ResetGrid?.Invoke();
                                                       UpdateVisibleFrames();
                                                   };

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

        public Action<int> ScrollToRow { protected get; set; }

        public Action ResetGrid { protected get; set; }

        public BindableCollection<FrameViewModel> VisibleFrames { get; } = new BindableCollection<FrameViewModel>();

        public event EventHandler<(IList<FrameViewModel> Queries, bool ToSecondary)> FramesForQueryChanged;
        public event EventHandler<FrameViewModel> FrameForVideoChanged;
        public event EventHandler<FrameViewModel> FrameForScrollVideoChanged;
        public event EventHandler<FrameViewModel> FrameForSortChanged;
        public event EventHandler<FrameViewModel> FrameForGpsChanged;
        public event EventHandler<IList<FrameViewModel>> SubmittedFramesChanged;

        public int[] GetTopFrameIds(int count) => _loadedFrames.Select(GetFrameId).Where(id => id.HasValue).Take(count).Cast<int>().ToArray();

        public virtual async Task LoadVideoForFrame(FrameViewModel frameViewModel)
        {
            int videoId = frameViewModel.VideoId;
            _loadedFrames = await Task.Run(
                                () => _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumbersForVideo(videoId)
                                                             .Select(frameId => ConvertThumbnailToViewModel(videoId, frameId))
                                                             .ToList());
            SelectFrame(frameViewModel);
            UpdateVisibleFrames();
        }

        public virtual async Task LoadFramesForIds(IEnumerable<int> sortedFrameIds)
        {
            _loadedFrames = await Task.Run(() => sortedFrameIds.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());
            UpdateVisibleFrames();
        }

        public virtual async Task LoadInitialDisplay()
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            if (_datasetServicesManager.CurrentDataset.DatasetParameters.IsInitialDisplayPrecomputed)
            {
                IReadOnlyList<int> ids = _datasetServicesManager.CurrentDataset.InitialDisplayProvider.InitialDisplayIds;
                _loadedFrames = await Task.Run(() => ids.Select(GetFrameViewModelForFrameId).Where(f => f != null).ToList());
                RowCount = _datasetServicesManager.CurrentDataset.InitialDisplayProvider.RowCount;
                ColumnCount = _datasetServicesManager.CurrentDataset.InitialDisplayProvider.ColumnCount;
                ScrollToRow(0);
                AddFramesToVisibleItems(VisibleFrames, _loadedFrames);
            }
            else
            {
                Random random = new Random(); //shuffle initial images randomly
                _loadedFrames = await Task.Run(() => datasetService.VideoIds.SelectMany(LoadAllThumbnails).OrderBy(_ => random.Next()).ToList());
                UpdateVisibleFrames();
            }
        }

        public void OnAddToQueryClicked(FrameViewModel frameViewModel)
        {
            frameViewModel.IsSelectedForQuery = true;
            BeforeEventAction();
            var toOther = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            FramesForQueryChanged?.Invoke(this, (_loadedFrames.Where(f => f.IsSelectedForQuery).Append(frameViewModel).Distinct().ToList(), toOther));
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
                EnumerableExtensions.ForEach(_loadedFrames, f => f.IsSelectedForQuery = false);
            }
            
        }

        public void OnFramesSubmitted(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            SubmittedFramesChanged?.Invoke(
                this,
                _loadedFrames.Where(f => f.IsSelectedForQuery)
                             .Append(frameViewModel)
                             .Distinct()
                             .Select(f => new FrameViewModel(f.VideoId, f.FrameNumber, _datasetServicesManager))
                             .ToList());
        }

        public void OnSortDisplay(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            FrameForSortChanged?.Invoke(this, frameViewModel);
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

        protected virtual void BeforeEventAction()
        {
        }

        protected virtual void UpdateVisibleFrames()
        {
            AddFramesToVisibleItems(VisibleFrames, _loadedFrames);
        }

        protected virtual void AddFramesToVisibleItems(BindableCollection<FrameViewModel> collectionToUpdate, IList<FrameViewModel> viewModelsToAdd)
        {
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

        protected FrameViewModel ConvertThumbnailToViewModel(int videoId, int frameNumber)
        {
            return new FrameViewModel(videoId, frameNumber, _datasetServicesManager);
        }

        protected int? GetFrameId(FrameViewModel frame)
        {
            return !_datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(frame.VideoId, frame.FrameNumber, out int frameId) ? (int?)null : frameId;
        }

        private IEnumerable<FrameViewModel> LoadAllThumbnails(int videoId)
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            return datasetService.GetFrameNumbersForVideo(videoId).Select(frameNumber => ConvertThumbnailToViewModel(videoId, frameNumber));
        }
        
        private FrameViewModel GetFrameViewModelForFrameId(int frameId)
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            int videoId = datasetService.GetVideoIdForFrameId(frameId);
            int frameNumber = datasetService.GetFrameNumberForFrameId(frameId);
            return ConvertThumbnailToViewModel(videoId, frameNumber);
        }
    }
}
