﻿using System;
using System.Linq;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.Services;
using System.Windows.Media;

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
        private bool _isLastInVideo;

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


        public int FrameNumber { get; private set; }

        public virtual byte[] ImageSource => _servicesManager.CurrentDataset.ThumbnailService.GetThumbnail(VideoId, FrameNumber).Image;

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

        // TODO: check usage
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
