using System;
using System.Linq;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.PresentationLayer.Controls.Common
{
    public class FrameViewModel : PropertyChangedBase
    {
        private readonly Lazy<int[]> _framesInTheVideo;
        private readonly IDatasetServicesManager _servicesManager;
        private bool _isSelectedForDetail;
        private bool _isSelectedForQuery;
        private bool _isVisible = true;

        public FrameViewModel(int videoId, int frameNumber, IDatasetServicesManager servicesManager)
        {
            _servicesManager = servicesManager;
            VideoId = videoId;
            FrameNumber = frameNumber;

            _framesInTheVideo = new Lazy<int[]>(() => servicesManager.CurrentDataset.ThumbnailService.GetThumbnails(VideoId).Select(t => t.FrameNumber).ToArray());
        }

        public bool CanAddToQuery => _servicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(VideoId, FrameNumber, out _);

        public int FrameNumber { get; private set; }

        public byte[] ImageSource
        {
            get
            {
                return _servicesManager.CurrentDataset.ThumbnailService.GetThumbnail(VideoId, FrameNumber).Image;
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

        public int VideoId { get; }

        public FrameViewModel Clone()
        {
            return new FrameViewModel(VideoId, FrameNumber, _servicesManager);
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
            NotifyOfPropertyChange(nameof(ImageSource));
            NotifyOfPropertyChange(nameof(CanAddToQuery));
        }
    }
}
