using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.RankingModels;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.SubmitControl.ViewModels;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public abstract class DisplayControlViewModelBase : PropertyChangedBase
    {
        protected readonly IDatasetServicesManager _datasetServicesManager;
        private readonly IWindowManager _windowManager;
        private readonly SubmitControlViewModel _submitControlViewModel;

        private bool _isLargeDisplayChecked;
        private bool _isShowFilteredVideosChecked;
        private bool _isSortDisplayChecked;

        protected readonly ILogger _logger;
        protected List<FrameViewModel> _loadedFrames = new List<FrameViewModel>();

        protected DisplayControlViewModelBase(ILogger logger, IDatasetServicesManager datasetServicesManager, IWindowManager windowManager, SubmitControlViewModel submitControlViewModel)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            _windowManager = windowManager;
            _submitControlViewModel = submitControlViewModel;
            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight);
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth);
        }

        public int DisplayHeight { get; set; }

        public Action DisplaySizeChangedHandler => UpdateVisibleFrames;

        public int DisplayWidth { get; set; }

        public int ImageHeight { get; }
        public int ImageWidth { get; }

        public bool IsLargeDisplayChecked
        {
            get => _isLargeDisplayChecked;
            set
            {
                if (_isLargeDisplayChecked == value)
                {
                    return;
                }

                _isLargeDisplayChecked = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsShowFilteredVideosChecked
        {
            get => _isShowFilteredVideosChecked;
            set
            {
                if (_isShowFilteredVideosChecked == value)
                {
                    return;
                }

                _isShowFilteredVideosChecked = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsSortDisplayChecked
        {
            get => _isSortDisplayChecked;
            set
            {
                if (_isSortDisplayChecked == value)
                {
                    return;
                }

                _isSortDisplayChecked = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<FrameViewModel> VisibleFrames { get; } = new BindableCollection<FrameViewModel>();

        public event EventHandler<FrameViewModel> SelectedFrameChanged;

        public event EventHandler<(FrameViewModel SelectedFrame, IList<int> VisibleFrameIds)> SortFrames;

        public event EventHandler<IList<FrameViewModel>> FramesForQueryChanged;

        public void AddToQueryClicked(FrameViewModel frameViewModel)
        {
            frameViewModel.IsSelectedForQuery = true;
            FramesForQueryChanged?.Invoke(this, _loadedFrames.Where(f => f.IsSelectedForQuery).ToList());
        }

        public void FrameSelected(FrameViewModel frameViewModel)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                frameViewModel.IsSelectedForQuery = !frameViewModel.IsSelectedForQuery;
                return;
            }

            _loadedFrames.ForEach(f => f.IsSelectedForDetail = false);
            frameViewModel.IsSelectedForDetail = true;
            SelectedFrameChanged?.Invoke(this, frameViewModel);
        }

        public void FilterVideoButton()
        {
            //TODO
        }

        public virtual async Task LoadVideoForFrame(FrameViewModel frameViewModel)
        {
            _loadedFrames = await Task.Run(
                                () => _datasetServicesManager.CurrentDataset.ThumbnailService.GetThumbnails(frameViewModel.VideoId)
                                                             .Select(t => ConvertThumbnailToViewModel(t.VideoId, t.FrameNumber))
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
            _loadedFrames = await Task.Run(() => datasetService.VideoIds.SelectMany(LoadThumbnails).OrderBy(_ => random.Next()).ToList());
            UpdateVisibleFrames();
        }

        public void FramesSubmitted(FrameViewModel frameViewModel)
        {
            _submitControlViewModel.Initialize(_loadedFrames.Where(f => f.IsSelectedForQuery).Append(frameViewModel).Distinct().Select(f => f.Clone()).ToList());
            if (_windowManager.ShowDialog(_submitControlViewModel) != true)
            {
                return;
            }

            _logger.Info($"Frames submitted: {string.Join(",", _submitControlViewModel.SubmittedFrames.Select(f => f.FrameNumber))}");
            MessageBox.Show("Frames submitted");
            //TODO send SubmittedFrames.Select(...) somewhere
        }

        public void OnSortDisplay(FrameViewModel frameViewModel)
        {
            const int topFramesCount = 1000;
            SortFrames?.Invoke(this, (frameViewModel, _loadedFrames.Take(topFramesCount).Select(GetFrameId).Where(t => t.HasValue).Select(t => t.Value).ToList()));
        }

        protected abstract void UpdateVisibleFrames();

        protected FrameViewModel SelectFrame(FrameViewModel frame)
        {
            int? frameId = GetFrameId(frame);
            if (!frameId.HasValue)
            {
                return null;
            }

            FrameViewModel selectedFrame = _loadedFrames.SingleOrDefault(f => GetFrameId(f) == frameId);
            if (selectedFrame != null)
            {
                selectedFrame.IsSelectedForDetail = true;
            }

            return selectedFrame;
        }

        private FrameViewModel ConvertThumbnailToViewModel(int videoId, int frameNumber)
        {
            return new FrameViewModel(videoId, frameNumber, _datasetServicesManager.CurrentDataset.ThumbnailService);
        }

        private IEnumerable<FrameViewModel> LoadThumbnails(int videoId)
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

        private int? GetFrameId(FrameViewModel frame)
        {
            return !_datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(frame.VideoId, frame.FrameNumber, out int frameId) ? (int?)null : frameId;
        }
    }
}
