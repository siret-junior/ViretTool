using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Action = System.Action;

namespace ViretTool.PresentationLayer.Controls.DisplayControl.ViewModels
{
    public class DisplayControlViewModel : PropertyChangedBase
    {
        private List<TileViewModel> _selectedTiles = new List<TileViewModel>();
        private List<TileViewModel> _submittedTiles = new List<TileViewModel>();
        private List<TileViewModel> _loadedTiles = new List<TileViewModel>();

        private bool _isSortDisplayChecked;
        private bool _isLargeDisplayChecked;
        private bool _isShowFilteredVideosChecked;
        private int _currentPageNumber;

        public DisplayControlViewModel()
        {

        }

        public BindableCollection<TileViewModel> VisibleTiles { get; } = new BindableCollection<TileViewModel>();

        public int ImageHeight { get; } = 75;
        public int ImageWidth { get; } = 100;

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

        public int DisplayWidth { get; set; }
        public int DisplayHeight { get; set; }

        public Action DisplaySizeChangedHandler => UpdateVisibleTiles;

        public void FilterVideoButton()
        {
            //TODO
        }

        public void FirstPageButton()
        {
            CurrentPageNumber = 0;
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

        public void NextPageButton()
        {
            CurrentPageNumber++;
            UpdateVisibleTiles();
        }

        public void LastPageButton()
        {
            var itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            CurrentPageNumber = (int)Math.Ceiling(_loadedTiles.Count / (double)itemsCount) - 1;
            UpdateVisibleTiles();
        }

        public void AddClicked(TileViewModel tileViewModel)
        {
            tileViewModel.IsSelected = true;
            _selectedTiles.Add(tileViewModel);
        }

        public void RemoveClicked(TileViewModel tileViewModel)
        {
            tileViewModel.IsSelected = false;
            _selectedTiles.Remove(tileViewModel);
        }

        public void AddSubmitClicked(TileViewModel tileViewModel)
        {
            _submittedTiles.Add(tileViewModel);
        }

        public void RemoveSubmitClicked(TileViewModel tileViewModel)
        {
            _submittedTiles.Remove(tileViewModel);
        }



        public void SubmitButton()
        {
            //TODO - submit _submittedTiles
        }

        public void UpdateVisibleTiles()
        {
            var itemsCount = (DisplayHeight / ImageHeight) * (DisplayWidth / ImageWidth);
            VisibleTiles.Clear();
            VisibleTiles.AddRange(_loadedTiles.Skip(CurrentPageNumber * itemsCount).Take(itemsCount));
        }


        public async Task LoadDataset(string databasePath)
        {
            await Task.Run(
                () =>
                {
                    _loadedTiles.Clear();
                    _loadedTiles.AddRange(LoadData(databasePath));
                });
            CurrentPageNumber = 0;
            UpdateVisibleTiles();
        }

        private IEnumerable<TileViewModel> LoadData(string databasePath)
        {
            for (int i = 0; i < 100000; i++)
            {
                BitmapImage imageSource = new BitmapImage(new Uri(@"d:\Temp\Downloads\signature.png"));
                imageSource.Freeze();
                yield return new TileViewModel(imageSource);
            }
        }

        
    }
}
