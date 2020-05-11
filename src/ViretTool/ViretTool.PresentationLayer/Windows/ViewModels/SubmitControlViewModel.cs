using System;
using System.Linq;
using System.Collections.Generic;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class SubmitControlViewModel : Screen
    {

        private readonly IDatasetServicesManager _datasetServicesManager;
        private int _imageHeight;
        private int _imageWidth;
        private bool _isTextFacesChecked = false;
        private bool _isTextChecked = false;
        private bool _isFacesChecked = false;
        private bool _isNothingChecked = true;
        private bool _isColorChecked = false;
        private string _aggregatedLabel = null;
        public SubmitControlViewModel(IDatasetServicesManager datasetServicesManager)
        {
            _datasetServicesManager = datasetServicesManager;
            datasetServicesManager.DatasetOpened += (_, services) =>
                                                    {
                                                        ImageHeight = services.DatasetParameters.DefaultFrameHeight;
                                                        ImageWidth = services.DatasetParameters.DefaultFrameWidth;
                                                    };
        }

        public event EventHandler<FrameViewModel> FrameForScrollVideoChanged;

        public void OnScrollVideoDisplay(FrameViewModel frameViewModel)
        {
            FrameForScrollVideoChanged?.Invoke(this, frameViewModel.Clone());
        }

        public int ImageHeight
        {
            get => _imageHeight;
            private set
            {
                _imageHeight = value;
                NotifyOfPropertyChange();
            }
        }

        public int ImageWidth
        {
            get => _imageWidth;
            private set
            {
                _imageWidth = value;
                NotifyOfPropertyChange();
            }
        }
        public bool IsTextChecked
        {
            get => _isTextChecked;
            set
            {
                _isTextChecked = value;

                if (value == true)
                {
                    UpdateOverlay(false, true, false);
                }

                NotifyOfPropertyChange();
            }
        }

        public bool IsFacesChecked
        {
            get => _isFacesChecked;
            set
            {
                _isFacesChecked = value;

                if (value == true)
                {
                    UpdateOverlay(true, false, false);
                }

                NotifyOfPropertyChange();
            }
        }
        public bool IsColorChecked
        {
            get => _isColorChecked;
            set
            {
                _isColorChecked = value;

                if (value == true)
                {
                    UpdateOverlay(false, false, true);
                }

                NotifyOfPropertyChange();
            }
        }
        public bool IsTextFacesChecked
        {
            get => _isTextFacesChecked;
            set
            {
                _isTextFacesChecked = value;

                if (value == true)
                {
                    UpdateOverlay(true, true, false);
                }

                NotifyOfPropertyChange();
            }
        }

        public bool IsNothingChecked
        {
            get => _isNothingChecked;
            set
            {
                _isNothingChecked = value;

                if (value == true)
                {
                    UpdateOverlay(false, false, false);
                }

                NotifyOfPropertyChange();
            }
        }

        public string AggregatedLabel
        {
            get => _aggregatedLabel;
            set
            {
                if(value != _aggregatedLabel)
                {
                    _aggregatedLabel = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        private void UpdateOverlay(bool showFaces, bool showText, bool showColor)
        {
            foreach(FrameViewModel frame in SubmittedFrames)
            {
                frame.ShowOverlay(showFaces, showText, showColor);
            }
        }
        public BindableCollection<FrameViewModel> SubmittedFrames { get; } = new BindableCollection<FrameViewModel>();

        public void Initialize(IList<FrameViewModel> selectedFrames)
        {
            SubmittedFrames.Clear();
            SubmittedFrames.AddRange(selectedFrames);

            List<int> synsets = new List<int>();

            foreach (FrameViewModel frame in selectedFrames)
            {
                _datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(frame.VideoId, frame.FrameNumber, out int frameId);
                synsets.AddRange(_datasetServicesManager.CurrentDataset.KeywordSynsetProvider.GetDescriptor(frameId).Take(5).Select(x => x.synsetId).ToList());
            }
            var aggregatedSynsetIDs = from synsetID in synsets
                    group synsetID by synsetID into groups
                    let count = groups.Count()
                    orderby count descending
                    select groups.Key;
            AggregatedLabel = string.Join(", ", aggregatedSynsetIDs.ToArray()
                    .Select(synsetID =>
                        $"{_datasetServicesManager.CurrentDataset.KeywordLabelProvider.GetLabel(synsetID)}")
                    );
        }

        public void RemoveFromQueryClicked(FrameViewModel frameViewModel)
        {
            SubmittedFrames.Remove(frameViewModel);
        }

        public void Ok()
        {
            TryClose(true);
        }

        public void Cancel()
        {
            TryClose(false);
        }
    }
}
