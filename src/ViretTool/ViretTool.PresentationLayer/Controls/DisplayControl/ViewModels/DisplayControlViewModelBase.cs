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
using ViretTool.BusinessLayer.Thumbnails;
using ViretTool.PresentationLayer.Controls.Common;
using ViretTool.PresentationLayer.Controls.SubmitControl.ViewModels;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public abstract class DisplayControlViewModelBase : PropertyChangedBase
    {
        private readonly IDatasetServicesManager _datasetServicesManager;
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
            _loadedFrames = await Task.Run(() => LoadThumbnails(frameViewModel.VideoId).ToList());
            _loadedFrames.Single(f => f.FrameNumber == frameViewModel.FrameNumber).IsSelectedForDetail = true;
            UpdateVisibleFrames();
        }

        public virtual async Task LoadFramesFromQueryResult(TemporalRankedResultSet frameViewModel)
        {
            //TODO - combine both results
            _loadedFrames = await Task.Run(
                                () => frameViewModel.TemporalResultSets.First()
                                                    .Take(10000)
                                                    .Select(r => GetFrameViewModelForFrameIdCached(r.Id))
                                                    .Where(f => f != null)
                                                    .ToList());
            UpdateVisibleFrames();
        }

        public virtual async Task LoadInitialDisplay()
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            const int videoCount = 2;

            Random random = new Random(); //shuffle initial images randomly
            _loadedFrames = await Task.Run(
                                () => datasetService.VideoIds.OrderBy(_ => random.Next())
                                                    .Take(videoCount)
                                                    .SelectMany(LoadThumbnails)
                                                    .Where(f => datasetService.TryGetFrameIdForFrameNumber(f.VideoId, f.FrameNumber, out _))
                                                    .OrderBy(_ => random.Next())
                                                    .ToList());
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

        protected abstract void UpdateVisibleFrames();

        private FrameViewModel ConvertThumbnailToViewModel(Thumbnail<byte[]> thumbnail)
        {
            int videoId = thumbnail.VideoId;
            return new FrameViewModel(
                thumbnail.Image,
                videoId,
                thumbnail.FrameNumber,
                new Lazy<(int[] AllFrameNumbers, byte[][] FrameDataFromSameVideo)>(() => GetFramesFromSameVideo(videoId)));
        }

        private IEnumerable<FrameViewModel> LoadThumbnails(int videoId)
        {
            return _datasetServicesManager.CurrentDataset.ThumbnailService.GetThumbnails(videoId).Select(ConvertThumbnailToViewModel);
        }

        private (int[] frameNumbers, byte[][] dataFromSameVideo) GetFramesFromSameVideo(int videoId)
        {
            Thumbnail<byte[]>[] thumbnails = _datasetServicesManager.CurrentDataset.ThumbnailService.GetThumbnails(videoId);
            int[] frameNumbers = thumbnails.Select(t => t.FrameNumber).ToArray();
            byte[][] dataFromSameVideo = thumbnails.Select(t => t.Image).ToArray();
            return (frameNumbers, dataFromSameVideo);
        }

        private FrameViewModel GetFrameViewModelForFrameIdCached(int frameId)
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            int videoId = datasetService.GetVideoIdForFrameId(frameId);
            int frameNumber = datasetService.GetFrameNumberForFrameId(frameId);
            Thumbnail<byte[]> thumbnail = _datasetServicesManager.CurrentDataset.ThumbnailService.GetThumbnail(videoId, frameNumber);
            return ConvertThumbnailToViewModel(thumbnail);
        }
    }
}
