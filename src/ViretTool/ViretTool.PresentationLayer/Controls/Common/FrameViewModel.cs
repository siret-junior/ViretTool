using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Thumbnails;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public class FrameViewModel : PropertyChangedBase
    {
        private bool _isSelectedForQuery;
        private bool _isSelectedForDetail;
        private readonly IThumbnailService<Thumbnail<byte[]>> _thumbnailService;
        private readonly Lazy<int[]> _framesInTheVideo;

        public FrameViewModel(int videoId, int frameNumber, IThumbnailService<Thumbnail<byte[]>> thumbnailService)
        {
            _thumbnailService = thumbnailService;
            VideoId = videoId;
            FrameNumber = frameNumber;

            _framesInTheVideo = new Lazy<int[]>(() => _thumbnailService.GetThumbnails(VideoId).Select(t => t.FrameNumber).ToArray());
        }

        public FrameViewModel Clone()
        {
            return new FrameViewModel(VideoId, FrameNumber, _thumbnailService);
        }

        public int FrameNumber { get; private set; }

        public BitmapImage ImageSource
        {
            get
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(_thumbnailService.GetThumbnail(VideoId, FrameNumber).Image);
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
            int[] allFrameNumbers = _framesInTheVideo.Value;
            int newIndex = Array.IndexOf(allFrameNumbers, FrameNumber) + position;
            if (newIndex < 0 || newIndex >= allFrameNumbers.Length)
            {
                return;
            }

            FrameNumber = allFrameNumbers[newIndex];
            NotifyOfPropertyChange(nameof(ImageSource));
        }
    }
}
