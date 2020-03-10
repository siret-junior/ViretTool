using System;
using System.Linq;
using System.Drawing;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.Services;
using ViretTool.Core;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public class FrameViewModel : PropertyChangedBase
    {
        private const string EmptyKeywordsLabel = "-";

        private readonly int _originalFrameNumber;
        private readonly Lazy<int[]> _framesInTheVideo;
        private readonly IDatasetServicesManager _servicesManager;
        private bool _isSelectedForDetail;
        private bool _isSelectedForQuery;
        private bool _isVisible = true;
        private bool _isRightBorderVisible = false;
        private bool _isBottomBorderVisible = false;
        private System.Windows.Media.Color _rightBorderColor = Colors.Brown;
        private System.Windows.Media.Color _bottomBorderColor = Colors.Green;
        private bool _isLastInVideo;
        private bool _areFacesShown = false;
        private bool _isTextShown = false;
        private bool _isColorShown = false;
        private BitmapSource _facesOverlay = null;
        private BitmapSource _textOverlay = null;
        private BitmapSource _colorOverlay = null;

        public FrameViewModel(int videoId, int frameNumber, IDatasetServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
            VideoId = videoId;
            FrameNumber = _originalFrameNumber = frameNumber;

            _framesInTheVideo = new Lazy<int[]>(() => servicesManager.CurrentDataset.ThumbnailService.GetThumbnails(VideoId).Select(t => t.FrameNumber).ToArray());

        }

        public bool CanAddToQuery => _servicesManager.IsDatasetOpened && _servicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(VideoId, FrameNumber, out _);

        public bool GpsAddVisible => _servicesManager.IsDatasetOpened && _servicesManager.CurrentDataset.DatasetParameters.IsLifelogData;

        public string FrameMetadata
        {
            get
            {
                if (!_servicesManager.IsDatasetOpened || !_servicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(VideoId, FrameNumber, out int frameId))
                {
                    return string.Empty;
                }

                if (_servicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
                {
                    LifelogFrameMetadata metadata = _servicesManager.CurrentDataset.LifelogDescriptorProvider[frameId];
                    return $"{metadata.Time.Hours}:{metadata.Time.Minutes:D2}";
                }

                return _servicesManager.CurrentDataset.DatasetService.GetShotNumberForFrameId(frameId).ToString();
            }
        }

        public bool CanSubmit
        {
            get
            {
                if (!_servicesManager.IsDatasetOpened)
                {
                    return false;
                }

                if (!_servicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
                {
                    return true;
                }

                if (!_servicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(VideoId, FrameNumber, out int frameId))
                {
                    return false;
                }

                return !_servicesManager.CurrentDataset.LifelogDescriptorProvider[frameId].FromVideo;
            }
        }
        private void _computeFacesOverlay()
        {
            
            if (_servicesManager.IsDatasetOpened)
            {
                int width = 26;
                int height = 15;

                Bitmap _facesOverlay = new Bitmap(width, height);

                int frameID = _servicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(VideoId, FrameNumber);
                bool[] facesSignatureDescriptor = _servicesManager.CurrentDataset.FaceSignatureProvider.Descriptors[frameID];
                int count = _servicesManager.CurrentDataset.FaceSignatureProvider.DescriptorCount;

                if (height * width > count)
                {
                    throw new ArgumentOutOfRangeException("Wrong format of Thumbnail resolution or FacesSignatureDescriptor!");
                }

                int iterator = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (facesSignatureDescriptor[iterator++])
                        {
                            _facesOverlay.SetPixel(x, y, System.Drawing.Color.Red);
                        }
                    }
                }

                FacesOverlay = _facesOverlay.ToBitmapSource();
            }
        }
        private void _computeTextOverlay()
        {

            if (_servicesManager.IsDatasetOpened)
            {
                int width = 26;
                int height = 15;

                Bitmap _textOverlay = new Bitmap(width, height);

                int frameID = _servicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(VideoId, FrameNumber);
                bool[] textSignatureDescriptor = _servicesManager.CurrentDataset.TextSignatureProvider.Descriptors[frameID];
                int count = _servicesManager.CurrentDataset.FaceSignatureProvider.DescriptorCount;

                if (height * width > count)
                {
                    throw new ArgumentOutOfRangeException("Wrong format of Thumbnail resolution or TextSignatureDescriptor!");
                }

                int iterator = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (textSignatureDescriptor[iterator++])
                        {
                            _textOverlay.SetPixel(x, y, System.Drawing.Color.Green);
                        }
                    }
                }

                TextOverlay = _textOverlay.ToBitmapSource();

            }
        }
        private void _computeColorOverlay()
        {

            if (_servicesManager.IsDatasetOpened)
            {
                int width = 26;
                int height = 15;

                Bitmap _colorOverlay = new Bitmap(width, height);


                int frameID = _servicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(VideoId, FrameNumber);
                byte[] a = _servicesManager.CurrentDataset.ColorSignatureProvider.Descriptors[frameID];
                int count = _servicesManager.CurrentDataset.FaceSignatureProvider.DescriptorCount;

                if (height * width > count)
                {
                    throw new ArgumentOutOfRangeException("Wrong format of Thumbnail resolution or ColorSignatureDescriptors!");
                }

                // TODO: populate bitmap with valid values
                int iterator = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // just blue color for now
                        _colorOverlay.SetPixel(x, y, System.Drawing.Color.Blue);
                        
                    }
                }

                ColorOverlay = _colorOverlay.ToBitmapSource();
            }
        }
        public bool AreFacesShown
        {
            get => _areFacesShown;
            set
            {
                _areFacesShown = value;
                NotifyOfPropertyChange(() => AreFacesShown);
            }
        }
        public bool IsTextShown
        {
            get => _isTextShown;
            set
            {
                _isTextShown = value;
                NotifyOfPropertyChange(() => IsTextShown);
            }
        }

        public bool IsColorShown
        {
            get => _isColorShown;
            set
            {
                _isColorShown = value;
                NotifyOfPropertyChange(() => IsColorShown);
            }
        }


        public void ShowOverlay(bool showFaces, bool showText, bool showColor)
        {
            if(showFaces && FacesOverlay == null)
            {
                _computeFacesOverlay();
            }
            if (showText && TextOverlay == null)
            {
                _computeTextOverlay();
            }
            if(showColor && ColorOverlay == null)
            {
                _computeColorOverlay();
            }
            
            IsTextShown = showText;
            IsColorShown = showColor;
            AreFacesShown = showFaces;
            NotifyOfPropertyChange();
        }

        public int FrameNumber { get; private set; }

        public virtual byte[] ImageSource => _servicesManager.CurrentDataset.ThumbnailService.GetThumbnail(VideoId, FrameNumber).Image;
        
        public BitmapSource FacesOverlay
        {
            get => _facesOverlay;
            set
            {
                if(_facesOverlay != value)
                {
                    _facesOverlay = value;
                    NotifyOfPropertyChange(() => FacesOverlay);
                }
            }
        }
        public BitmapSource TextOverlay
        {
            get => _textOverlay;
            set
            {
                if (_textOverlay != value)
                {
                    _textOverlay = value;
                    NotifyOfPropertyChange(() => TextOverlay);
                }
            }
        }
        public BitmapSource ColorOverlay
        {
            get => _colorOverlay;
            set
            {
                if (_colorOverlay != value)
                {
                    _colorOverlay = value;
                    NotifyOfPropertyChange(() => ColorOverlay);
                }
            }
        }

        public bool IsSelectedForDetail
        {
            get => _isSelectedForDetail;
            set
            {
                if (_isSelectedForDetail == value)
                {
                    return;
                }

                _isSelectedForDetail = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsSelectedForQuery
        {
            get => _isSelectedForQuery;
            set
            {
                if (_isSelectedForQuery == value)
                {
                    return;
                }

                _isSelectedForQuery = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                _isVisible = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsRightBorderVisible
        {
            get => _isRightBorderVisible;
            set
            {
                if (_isRightBorderVisible == value)
                {
                    return;
                }

                _isRightBorderVisible = value;
                NotifyOfPropertyChange();
            }
        }
        public System.Windows.Media.Color RightBorderColor
        {
            get => _rightBorderColor;
            set
            {
                if (_rightBorderColor == value)
                {
                    return;
                }

                _rightBorderColor = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsBottomBorderVisible
        {
            get => _isBottomBorderVisible;
            set
            {
                if (_isBottomBorderVisible == value)
                {
                    return;
                }

                _isBottomBorderVisible = value;
                NotifyOfPropertyChange();
            }
        }
        public System.Windows.Media.Color BottomBorderColor
        {
            get => _bottomBorderColor;
            set
            {
                if (_bottomBorderColor == value)
                {
                    return;
                }

                _bottomBorderColor = value;
                NotifyOfPropertyChange();
            }
        }
        public int VideoId { get; }

        public string Label
        {
            get
            {
                if (!_servicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(VideoId, FrameNumber, out int frameId))
                {
                    return EmptyKeywordsLabel;
                }

                (int synsetId, float probability)[] synsets = _servicesManager.CurrentDataset.KeywordSynsetProvider.GetDescriptor(frameId);
                if (synsets == null || synsets.Length == 0)
                {
                    return EmptyKeywordsLabel;
                }

                // Top 1
                // return _servicesManager.CurrentDataset.KeywordLabelProvider.GetLabel(synsets.First().synsetId);

                // Top 5
                IKeywordLabelProvider<string> labelProvider = _servicesManager.CurrentDataset.KeywordLabelProvider;
                string label = string.Join(", ", synsets
                    .Take(5)
                    .Select(synset => 
                        $"{labelProvider.GetLabel(synset.synsetId)}")
                    );
                return label;
            }
        }

        public bool IsLastInVideo
        {
            get => _isLastInVideo;
            set
            {
                if (_isLastInVideo == value)
                {
                    return;
                }

                _isLastInVideo = value;
                NotifyOfPropertyChange();
            }
        }

        public FrameViewModel Clone()
        {
            return new FrameViewModel(VideoId, FrameNumber, _servicesManager);
        }

        public void ResetFrameNumber()
        {
            if (FrameNumber == _originalFrameNumber)
            {
                return;
            }

            FrameNumber = _originalFrameNumber;
            NotifyOfPropertyChange(nameof(CanSubmit));
            NotifyOfPropertyChange(nameof(ImageSource));
            NotifyOfPropertyChange(nameof(CanAddToQuery));
            NotifyOfPropertyChange(nameof(FrameMetadata));
            NotifyOfPropertyChange(nameof(Label));
        }

        public void ScrollNext()
        {
            ReloadBitmap(1);
        }

        public void ScrollPrevious()
        {
            ReloadBitmap(-1);
        }

        //+1 or -1 position
        private void ReloadBitmap(int position)
        {
            int[] allFrameNumbers = _framesInTheVideo.Value;
            int newIndex = Array.IndexOf(allFrameNumbers, FrameNumber) + position;
            if (newIndex < 0 || newIndex >= allFrameNumbers.Length)
            {
                return;
            }

            FrameNumber = allFrameNumbers[newIndex];
            NotifyOfPropertyChange(nameof(CanSubmit));
            NotifyOfPropertyChange(nameof(ImageSource));
            NotifyOfPropertyChange(nameof(CanAddToQuery));
            NotifyOfPropertyChange(nameof(FrameMetadata));
            NotifyOfPropertyChange(nameof(Label));
        }
    }
}
