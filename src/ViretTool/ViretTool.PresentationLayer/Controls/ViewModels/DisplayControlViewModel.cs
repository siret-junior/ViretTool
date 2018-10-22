using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace ViretTool.PresentationLayer.Controls.ViewModels
{
    public class DisplayControlViewModel : PropertyChangedBase
    {
        private List<TileViewModel> _selectedTiles = new List<TileViewModel>();
        private List<TileViewModel> _submittedTiles = new List<TileViewModel>();

        private bool _isSortDisplayChecked;
        private bool _isLargeDisplayChecked;
        private bool _isShowFilteredVideosChecked;
        private string _currentPageNumberText;

        public DisplayControlViewModel()
        {

        }

        public BindableCollection<TileViewModel> Tiles { get; } = new BindableCollection<TileViewModel>();

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

        public string CurrentPageNumberText
        {
            get => _currentPageNumberText;
            set
            {
                if (_currentPageNumberText == value)
                {
                    return;
                }

                _currentPageNumberText = value;
                NotifyOfPropertyChange();
            }
        }

        public void FilterVideoButton()
        {
            //TODO
        }

        public void FirstPageButton()
        {
            //TODO
        }

        public void PreviousPageButton()
        {
            //TODO
        }

        public void NextPageButton()
        {
            //TODO
        }

        public void LastPageButton()
        {
            //TODO
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

        public async Task LoadDataset(string databasePath)
        {
            Tiles.Clear();
            var data = await Task.Run(() => LoadData(databasePath).ToList());
            Tiles.AddRange(data);
        }

        public IEnumerable<TileViewModel> LoadData(string databasePath)
        {
            for (int i = 0; i < 1000; i++)
            {
                BitmapImage imageSource = new BitmapImage(new Uri(@"..."));
                imageSource.Freeze();
                yield return new TileViewModel(imageSource);
            }
        }
    }
}
