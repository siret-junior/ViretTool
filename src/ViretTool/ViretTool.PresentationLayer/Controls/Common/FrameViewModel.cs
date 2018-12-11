using System;
using System.IO;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public class FrameViewModel : PropertyChangedBase
    {
        private readonly int[] _allFrameNumbers;
        private readonly byte[][] _frameDataFromSameVideo;
        private bool _isSelectedForQuery;
        private bool _isSelectedForDetail;
        private byte[] _frameData;

        public FrameViewModel(byte[] frameData, int videoId, int frameNumber, int[] allFrameNumbers, byte[][] frameDataFromSameVideo)
        {
            _frameData = frameData;
            _allFrameNumbers = allFrameNumbers;
            _frameDataFromSameVideo = frameDataFromSameVideo;
            VideoId = videoId;
            FrameNumber = frameNumber;
        }

        public FrameViewModel Clone()
        {
            return new FrameViewModel(_frameData, VideoId, FrameNumber, _allFrameNumbers, _frameDataFromSameVideo);
        }

        public int FrameNumber { get; private set; }

        public BitmapImage ImageSource
        {
            get
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(_frameData);
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
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

        public int VideoId { get; }

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
            int newIndex = Array.IndexOf(_allFrameNumbers, FrameNumber) + position;
            if (newIndex < 0 || newIndex >= _allFrameNumbers.Length)
            {
                return;
            }

            FrameNumber = _allFrameNumbers[newIndex];
            _frameData = _frameDataFromSameVideo[newIndex];
            NotifyOfPropertyChange(nameof(ImageSource));
        }
    }
}
