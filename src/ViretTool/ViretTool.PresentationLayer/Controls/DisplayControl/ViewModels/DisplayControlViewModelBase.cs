using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public abstract class DisplayControlViewModelBase : Screen
    {
        protected readonly IDatasetServicesManager _datasetServicesManager;
        protected readonly ILogger _logger;
        protected List<FrameViewModel> _loadedFrames = new List<FrameViewModel>();

        protected DisplayControlViewModelBase(ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight);
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth);
        }

        public int DisplayHeight { get; set; }

        public int DisplayWidth { get; set; }

        public Action DisplaySizeChangedHandler => UpdateVisibleFrames;

        public int ImageHeight { get; }
        public int ImageWidth { get; }
        
        public BindableCollection<FrameViewModel> VisibleFrames { get; } = new BindableCollection<FrameViewModel>();

        public event EventHandler<IList<FrameViewModel>> FramesForQueryChanged;
        public event EventHandler<FrameViewModel> FrameForVideoChanged;
        public event EventHandler<FrameViewModel> FrameForScrollVideoChanged;
        public event EventHandler<(FrameViewModel SelectedFrame, IList<FrameViewModel> TopFrames)> FrameForSortChanged;
        public event EventHandler<IList<FrameViewModel>> SubmittedFramesChanged;


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

            Random random = new Random(); //shuffle initial images randomly
            _loadedFrames = await Task.Run(() => datasetService.VideoIds.SelectMany(LoadAllThumbnails).OrderBy(_ => random.Next()).ToList());
            UpdateVisibleFrames();
        }

        public void OnAddToQueryClicked(FrameViewModel frameViewModel)
        {
            frameViewModel.IsSelectedForQuery = true;
            BeforeEventAction();
            FramesForQueryChanged?.Invoke(this, _loadedFrames.Where(f => f.IsSelectedForQuery).ToList());
        }

        public void OnFrameSelected(FrameViewModel frameViewModel)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                frameViewModel.IsSelectedForQuery = !frameViewModel.IsSelectedForQuery;
            }
        }

        public void OnFramesSubmitted(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            SubmittedFramesChanged?.Invoke(this, _loadedFrames.Where(f => f.IsSelectedForQuery).Append(frameViewModel).Distinct().ToList());
        }

        public void OnSortDisplay(FrameViewModel frameViewModel)
        {
            BeforeEventAction();
            const int topFramesCount = 2000;
            FrameForSortChanged?.Invoke(this, (frameViewModel, _loadedFrames.Take(topFramesCount).ToList()));
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

        protected virtual void BeforeEventAction()
        {
        }

        protected virtual void UpdateVisibleFrames()
        {
            AddFramesToVisibleItems(_loadedFrames);
        }

        protected void AddFramesToVisibleItems(IList<FrameViewModel> viewModelsToAdd)
        {
            int i = 0;
            for (; i < VisibleFrames.Count && i < viewModelsToAdd.Count; i++)
            {
                VisibleFrames[i] = viewModelsToAdd[i];
                VisibleFrames[i].IsVisible = true;
            }

            if (VisibleFrames.Count < viewModelsToAdd.Count)
            {
                VisibleFrames.AddRange(viewModelsToAdd.Skip(VisibleFrames.Count));
            }
            else if (viewModelsToAdd.Count < VisibleFrames.Count)
            {
                for (; i < VisibleFrames.Count; i++)
                {
                    VisibleFrames[i].IsVisible = false;
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

        protected int? GetFrameId(FrameViewModel frame)
        {
            return !_datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(frame.VideoId, frame.FrameNumber, out int frameId) ? (int?)null : frameId;
        }
    }
}
