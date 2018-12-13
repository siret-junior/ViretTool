using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Thumbnails;
using ViretTool.PresentationLayer.Controls.Common;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public abstract class DisplayControlViewModelBase : PropertyChangedBase
    {
        private readonly IDatasetServicesManager _datasetServicesManager;
        private int _currentPageNumber;
        private bool _isLargeDisplayChecked;
        private bool _isShowFilteredVideosChecked;

        private bool _isSortDisplayChecked;
        protected List<FrameViewModel> _loadedFrames = new List<FrameViewModel>();

        protected DisplayControlViewModelBase(IDatasetServicesManager datasetServicesManager)
        {
            _datasetServicesManager = datasetServicesManager;
            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight);
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth);
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

        protected abstract void UpdateVisibleFrames();

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

        public async Task Load(int videoId)
        {
            _loadedFrames = await Task.Run(() => LoadThumbnails(videoId).ToList());
            CurrentPageNumber = 0;
            UpdateVisibleFrames();
        }

        public async Task LoadInitialDisplay()
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            const int videoCount = 10;

            Random random = new Random(); //shuffle initial images randomly
            _loadedFrames = await Task.Run(
                               () => datasetService.VideoIds.OrderBy(_ => random.Next()).Take(videoCount).SelectMany(LoadThumbnails).OrderBy(_ => random.Next()).ToList());
            CurrentPageNumber = 0;
            UpdateVisibleFrames();
        }

        public event EventHandler<FrameViewModel> SelectedFrameChanged;

        public event EventHandler<IList<FrameViewModel>> FramesForQueryChanged;


        public void FramesSubmitted()
        {
            //TODO - submit _submittedFrames
        }

        private FrameViewModel ConvertThumbnailToViewModel(Thumbnail<byte[]> thumbnail, Thumbnail<byte[]>[] thumbnailsFromSameVideo)
        {
            //TODO is wrong! int[] allFrameNumbers = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdsForVideo(thumbnail.VideoId);
            byte[][] frameDataFromSameVideo = thumbnailsFromSameVideo.Select(t => t.Image).ToArray();
            var allFrameNumbers = thumbnailsFromSameVideo.Select(t => t.FrameNumber).ToArray();
            return new FrameViewModel(thumbnail.Image, thumbnail.VideoId, thumbnail.FrameNumber, allFrameNumbers, frameDataFromSameVideo);
        }

        private IEnumerable<FrameViewModel> LoadThumbnails(int videoId)
        {
            Thumbnail<byte[]>[] thumbnails = _datasetServicesManager.CurrentDataset.ThumbnailService.GetThumbnails(videoId);
            return thumbnails.Select(t => ConvertThumbnailToViewModel(t, thumbnails));
        }
    }
}
