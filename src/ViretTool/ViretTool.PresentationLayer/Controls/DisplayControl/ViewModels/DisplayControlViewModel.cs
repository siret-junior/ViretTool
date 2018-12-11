using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Thumbnails;
using ViretTool.PresentationLayer.Controls.Common;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class DisplayControlViewModel : PropertyChangedBase
    {
        private readonly IDatasetServicesManager _datasetServicesManager;
        private int _currentPageNumber;
        private bool _isLargeDisplayChecked;
        private bool _isShowFilteredVideosChecked;

        private bool _isSortDisplayChecked;
        private List<FrameViewModel> _loadedFrames = new List<FrameViewModel>();

        private readonly List<FrameViewModel> _selectedFramesForQuery = new List<FrameViewModel>();
        private readonly List<FrameViewModel> _submittedFrames = new List<FrameViewModel>();

        public DisplayControlViewModel(IDatasetServicesManager datasetServicesManager)
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

        public void AddToQueryClicked(FrameViewModel frameViewModel)
        {
            if (!frameViewModel.IsSelectedForQuery)
            {
                ConsiderFrameToQueries(frameViewModel);
            }

            FramesForQueryChanged?.Invoke(this, _selectedFramesForQuery);
        }

        private void ConsiderFrameToQueries(FrameViewModel frameViewModel)
        {
            if (frameViewModel.IsSelectedForQuery)
            {
                _selectedFramesForQuery.Remove(frameViewModel);
            }
            else
            {
                _selectedFramesForQuery.Add(frameViewModel);
            }

            frameViewModel.IsSelectedForQuery = !frameViewModel.IsSelectedForQuery;
        }

        public void AddSubmitClicked(FrameViewModel frameViewModel)
        {
            _submittedFrames.Add(frameViewModel);
        }

        public void FrameSelected(FrameViewModel frameViewModel)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ConsiderFrameToQueries(frameViewModel);
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

        public void FirstPageButton()
        {
            CurrentPageNumber = 0;
            UpdateVisibleFrames();
        }

        public void LastPageButton()
        {
            var itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            CurrentPageNumber = (int)Math.Ceiling(_loadedFrames.Count / (double)itemsCount) - 1;
            UpdateVisibleFrames();
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

        public void NextPageButton()
        {
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

        public void RemoveClicked(FrameViewModel frameViewModel)
        {
            frameViewModel.IsSelectedForQuery = false;
            _selectedFramesForQuery.Remove(frameViewModel);
        }

        public void RemoveSubmitClicked(FrameViewModel frameViewModel)
        {
            _submittedFrames.Remove(frameViewModel);
        }

        public event EventHandler<FrameViewModel> SelectedFrameChanged;

        public event EventHandler<IList<FrameViewModel>> FramesForQueryChanged;


        public void SubmitButton()
        {
            //TODO - submit _submittedFrames
        }

        private FrameViewModel ConvertThumbnailToViewModel(Thumbnail<byte[]> thumbnail, byte[][] frameDataFromSameVideo)
        {
            int[] allFrameNumbers = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdsForVideo(thumbnail.VideoId);
            return new FrameViewModel(thumbnail.Image, thumbnail.VideoId, thumbnail.FrameNumber, allFrameNumbers, frameDataFromSameVideo);
        }

        private IEnumerable<FrameViewModel> LoadThumbnails(int videoId)
        {
            Thumbnail<byte[]>[] thumbnails = _datasetServicesManager.CurrentDataset.ThumbnailService.GetThumbnails(videoId);
            var frameDataFromSameVideo = thumbnails.Select(t => t.Image).ToArray();
            return thumbnails.Select(t => ConvertThumbnailToViewModel(t, frameDataFromSameVideo));
        }

        private void UpdateVisibleFrames()
        {
            var itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            VisibleFrames.Clear();
            VisibleFrames.AddRange(_loadedFrames.Skip(CurrentPageNumber * itemsCount).Take(itemsCount));
        }
    }
}
