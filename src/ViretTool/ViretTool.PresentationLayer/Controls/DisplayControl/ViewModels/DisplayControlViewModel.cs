using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Thumbnails;
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
        private List<TileViewModel> _loadedTiles = new List<TileViewModel>();

        private readonly List<TileViewModel> _selectedTiles = new List<TileViewModel>();
        private readonly List<TileViewModel> _submittedTiles = new List<TileViewModel>();

        public DisplayControlViewModel(IDatasetServicesManager datasetServicesManager)
        {
            _datasetServicesManager = datasetServicesManager;
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

        public Action DisplaySizeChangedHandler => UpdateVisibleTiles;

        public int DisplayWidth { get; set; }

        public int ImageHeight { get; } = 75;
        public int ImageWidth { get; } = 100;

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

        public BindableCollection<TileViewModel> VisibleTiles { get; } = new BindableCollection<TileViewModel>();

        public void AddClicked(TileViewModel tileViewModel)
        {
            tileViewModel.IsSelected = true;
            _selectedTiles.Add(tileViewModel);
        }

        public void AddSubmitClicked(TileViewModel tileViewModel)
        {
            _submittedTiles.Add(tileViewModel);
        }

        public void FilterVideoButton()
        {
            //TODO
        }

        public void FirstPageButton()
        {
            CurrentPageNumber = 0;
            UpdateVisibleTiles();
        }

        public void LastPageButton()
        {
            var itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            CurrentPageNumber = (int)Math.Ceiling(_loadedTiles.Count / (double)itemsCount) - 1;
            UpdateVisibleTiles();
        }

        public async Task Load(int videoId)
        {
            _loadedTiles = await Task.Run(() => LoadThumbnails(videoId).ToList());
            CurrentPageNumber = 0;
            UpdateVisibleTiles();
        }

        public async Task LoadInitialDisplay()
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            const int videoCount = 10;

            Random random = new Random(); //shuffle initial images randomly
            _loadedTiles = await Task.Run(
                               () => datasetService.VideoIds.OrderBy(_ => random.Next()).Take(videoCount).SelectMany(LoadThumbnails).OrderBy(_ => random.Next()).ToList());
            CurrentPageNumber = 0;
            UpdateVisibleTiles();
        }

        public void NextPageButton()
        {
            CurrentPageNumber++;
            UpdateVisibleTiles();
        }

        public void PreviousPageButton()
        {
            if (CurrentPageNumber <= 0)
            {
                return;
            }

            CurrentPageNumber--;
            UpdateVisibleTiles();
        }

        public void RemoveClicked(TileViewModel tileViewModel)
        {
            tileViewModel.IsSelected = false;
            _selectedTiles.Remove(tileViewModel);
        }

        public void RemoveSubmitClicked(TileViewModel tileViewModel)
        {
            _submittedTiles.Remove(tileViewModel);
        }

        public event EventHandler<TileViewModel> SelectedFrameChanged;


        public void SubmitButton()
        {
            //TODO - submit _submittedTiles
        }

        public void TileMouseDown(TileViewModel tileViewModel)
        {
            SelectedFrameChanged?.Invoke(this, tileViewModel);
        }

        private TileViewModel ConvertThumbnailToViewModel(Thumbnail<byte[]> thumbnail)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(thumbnail.Image);
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return new TileViewModel(bitmapImage, thumbnail.VideoId, thumbnail.FrameNumber);
        }

        private IEnumerable<TileViewModel> LoadThumbnails(int videoId)
        {
            Thumbnail<byte[]>[] thumbnails = _datasetServicesManager.CurrentDataset.ThumbnailService.GetThumbnails(videoId);
            return thumbnails.Select(ConvertThumbnailToViewModel);
        }

        private void UpdateVisibleTiles()
        {
            var itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            VisibleTiles.Clear();
            VisibleTiles.AddRange(_loadedTiles.Skip(CurrentPageNumber * itemsCount).Take(itemsCount));
        }
    }
}
