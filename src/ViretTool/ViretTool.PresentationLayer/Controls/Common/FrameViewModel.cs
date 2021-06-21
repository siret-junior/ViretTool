using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ViretTool.Core;
using ViretTool.BusinessLayer.Services;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using ViretTool.PresentationLayer.Helpers;

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
        private Color _rightBorderColor = Colors.Brown;
        private Color _bottomBorderColor = Colors.Green;
        //private bool _isLastInVideo;

        public FrameViewModel(int videoId, int frameNumber, IDatasetServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
            VideoId = videoId;
            FrameNumber = _originalFrameNumber = frameNumber;

            _framesInTheVideo = new Lazy<int[]>(() => servicesManager.CurrentDataset.ThumbnailService.ReadVideoFrameNumbers(VideoId));
        }

        public bool CanAddToQuery => _servicesManager.IsDatasetOpened && _servicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(VideoId, FrameNumber, out _);

        public string TopRightLabel
        {
            get
            {
                if (!_servicesManager.IsDatasetOpened || !_servicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(VideoId, FrameNumber, out int frameId))
                {
                    return string.Empty;
                }
                else
                {
                    //return _servicesManager.CurrentDataset.DatasetService.GetShotNumberForFrameId(frameId).ToString();
                    return Annotation;
                }
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
                else
                {
                    return true;
                }
            }
        }

        public int FrameNumber { get; private set; }

        public virtual byte[] ImageSource => _servicesManager.CurrentDataset.ThumbnailService.ReadThumbnail(VideoId, FrameNumber).ImageJpeg;


        public bool IsHighlighted => Annotation != null && !Annotation.Equals("");

        //private Brush[] _highlightColors = new Brush[] 
        //{ 
        //    Brushes.Red, Brushes.Lime, Brushes.Blue, 
        //    Brushes.Salmon, Brushes.PaleGreen, Brushes.LightBlue,
        //    Brushes.Cyan, Brushes.Magenta, Brushes.Yellow, Brushes.Orange, Brushes.Olive, Brushes.Silver
        //};

        public Brush HighlightColor { get; set; } = new SolidColorBrush(Color.FromRgb(0, 0, 0));

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

        /// <summary>
        /// True if right border is visible
        /// </summary>
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
        public Color RightBorderColor
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

        /// <summary>
        /// True if bottom border is visible
        /// </summary>
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

        /// <summary>
        /// Color of bottom border
        /// </summary>
        public Color BottomBorderColor
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

        public string Annotation { get; set; }

        private double _score = 0;
        //private double _scoreTop = 1.7;
        //private double _scoreBottom = 1;
        public double Score 
        {
            get => _score; 
            set 
            {
                if (_score == value)
                {
                    return;
                }
                _score = value;

                double _scoreTop = _servicesManager.ViretCore.Config.HighlightFrameGreenAt;
                double _scoreBottom = _servicesManager.ViretCore.Config.HighlightFrameRedAt;
                HighlightColor = new SolidColorBrush(ColorInterpolationHelper.InterpolateColorHSV((_scoreTop - _score) / (_scoreTop - _scoreBottom), true));
                NotifyOfPropertyChange();
            }
        }
        public string ToolTipLabel
        {
            get => $"{Annotation} ({Score.ToString("0.00")})";
        }

        // TODO: check usage
        //public bool IsLastInVideo
        //{
        //    get => _isLastInVideo;
        //    set
        //    {
        //        if (_isLastInVideo == value)
        //        {
        //            return;
        //        }

        //        _isLastInVideo = value;
        //        NotifyOfPropertyChange();
        //    }
        //}

        public FrameViewModel Clone()
        {
            return new FrameViewModel(VideoId, FrameNumber, _servicesManager);
        }

        /// <summary>
        /// Resets to the original keyframe after scrolling-browsing finished and mouse has been hovered out.
        /// </summary>
        public void ResetFrameNumber()
        {
            if (FrameNumber == _originalFrameNumber)
            {
                return;
            }
            ReloadWithFrameNumber(_originalFrameNumber);
        }

        public void ScrollToNextFrame()
        {
            ScrollToFrame(1);
        }

        public void ScrollToPreviousFrame()
        {
            ScrollToFrame(-1);
        }

        // +1 or -1 direction
        private void ScrollToFrame(int direction)
        {
            int[] videoFrameNumbers = _framesInTheVideo.Value;
            int newFrameIndex = Array.IndexOf(videoFrameNumbers, FrameNumber) + direction;
            if (newFrameIndex < 0 || newFrameIndex >= videoFrameNumbers.Length)
            {
                // attempting to scroll past the range of the video
                return;
            }
            ReloadWithFrameNumber(videoFrameNumbers[newFrameIndex]);
        }

        private void ReloadWithFrameNumber(int frameNumber)
        {
            FrameNumber = frameNumber;
            NotifyOfPropertyChange(nameof(CanSubmit));
            NotifyOfPropertyChange(nameof(ImageSource));
            NotifyOfPropertyChange(nameof(CanAddToQuery));
            NotifyOfPropertyChange(nameof(TopRightLabel));
            NotifyOfPropertyChange(nameof(ToolTipLabel));
        }
    }
}
