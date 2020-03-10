using System;
using System.Collections.Generic;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class SubmitControlViewModel : Screen
    {
        private int _imageHeight;
        private int _imageWidth;
        private bool _isTextFacesChecked = false;
        private bool _isTextChecked = false;
        private bool _isFacesChecked = false;
        private bool _isNothingChecked = true;
        private bool _isColorChecked = false;
        public SubmitControlViewModel(IDatasetServicesManager datasetServicesManager)
        {
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
                    _updateOverlay(false, true, false);
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
                    _updateOverlay(true, false, false);
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
                    _updateOverlay(false, false, true);
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
                    _updateOverlay(true, true, false);
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
                    _updateOverlay(false, false, false);
                }

                NotifyOfPropertyChange();
            }
        }

        private void _updateOverlay(bool showFaces, bool showText, bool showColor)
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
