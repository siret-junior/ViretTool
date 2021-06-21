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

        
        // TODO: fix FrameViewModel dependencies
        public double LargeFramesMultiplier => 2;

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
                                                        ImageHeight = 108;// services.DatasetParameters.DefaultFrameHeight;
                                                        ImageWidth = 192;// services.DatasetParameters.DefaultFrameWidth;
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

        


        /// <summary>
        /// Computed string of labels
        /// </summary>
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
        
        public BindableCollection<FrameViewModel> SubmittedFrames { get; } = new BindableCollection<FrameViewModel>();


        /// <summary>
        /// Adds frames to submit window and computes AggregatedLabel
        /// </summary>
        /// <param name="selectedFrames"></param>
        public void Initialize(IList<FrameViewModel> selectedFrames)
        {
            SubmittedFrames.Clear();
            SubmittedFrames.AddRange(selectedFrames);

            _isTextFacesChecked = false;
            _isTextChecked = false;
            _isFacesChecked = false;
            _isNothingChecked = true;
            _isColorChecked = false;
            _aggregatedLabel = null;

            // compute labels 
            //List<int> synsets = new List<int>();

            //foreach (FrameViewModel frame in selectedFrames)
            //{
            //    _datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(frame.VideoId, frame.FrameNumber, out int frameId);
            //    synsets.AddRange(_datasetServicesManager.CurrentDataset.KeywordSynsetProvider.GetDescriptor(frameId).Take(5).Select(x => x.synsetId).ToList());
            //}
            //IEnumerable<int> aggregatedSynsetIDs = from synsetID in synsets
            //        group synsetID by synsetID into groups
            //        let count = groups.Count()
            //        orderby count descending
            //        select groups.Key;
            //AggregatedLabel = string.Join(", ", aggregatedSynsetIDs.ToArray()
            //        .Select(synsetID =>
            //            $"{_datasetServicesManager.CurrentDataset.KeywordLabelProvider.GetLabel(synsetID)}")
            //        );
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
