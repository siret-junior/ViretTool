using System;
using System.IO;
using System.Windows.Media.Imaging;
using Caliburn.Micro;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public class FrameViewModel : PropertyChangedBase
    {
        private bool _isSelectedForQuery;
        private bool _isSelectedForDetail;
        private byte[] _frameData;
        private readonly Lazy<(int[] AllFrameNumbers, byte[][] FrameDataFromSameVideo)> _framesInTheVideo;

        public FrameViewModel(byte[] frameData, int videoId, int frameNumber, Lazy<(int[] AllFrameNumbers, byte[][] FrameDataFromSameVideo)> framesInTheVideo)
        {
            _frameData = frameData;
            _framesInTheVideo = framesInTheVideo;
            VideoId = videoId;
            FrameNumber = frameNumber;
        }

        public FrameViewModel Clone()
        {
            return new FrameViewModel(_frameData, VideoId, FrameNumber, _framesInTheVideo);
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
            (int[] allFrameNumbers, byte[][] frameDataFromSameVideo) = _framesInTheVideo.Value;
            int newIndex = Array.IndexOf(allFrameNumbers, FrameNumber) + position;
            if (newIndex < 0 || newIndex >= allFrameNumbers.Length)
            {
                return;
            }

            FrameNumber = allFrameNumbers[newIndex];
            _frameData = frameDataFromSameVideo[newIndex];
            NotifyOfPropertyChange(nameof(ImageSource));
        }
    }
}
