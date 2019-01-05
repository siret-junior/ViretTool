using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class TestControlViewModel : Screen
    {
        private readonly IDatasetServicesManager _datasetServicesManager;
        private readonly Random _random = new Random();
        private FrameViewModel _firstFrame;
        private FrameViewModel _secondFrame;

        public TestControlViewModel(IDatasetServicesManager datasetServicesManager)
        {
            _datasetServicesManager = datasetServicesManager;
            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight) * 2;
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth) * 2;
        }

        public int ImageHeight { get; }

        public int ImageWidth { get; }

        public FrameViewModel FirstFrame
        {
            get => _firstFrame;
            private set
            {
                if (_firstFrame == value)
                {
                    return;
                }

                _firstFrame = value;
                NotifyOfPropertyChange();
            }
        }

        public FrameViewModel SecondFrame
        {
            get => _secondFrame;
            private set
            {
                if (_secondFrame == value)
                {
                    return;
                }

                _secondFrame = value;
                NotifyOfPropertyChange();
            }
        }

        public void InitializeFramesRandomly()
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            int videoId = datasetService.VideoIds[_random.Next(datasetService.VideoIds.Length)];

            List<int> randomizedFrames = datasetService.GetFrameNumbersForVideo(videoId).OrderBy(_ => _random.Next()).Take(2).OrderBy(n => n).ToList();
            FirstFrame = new FrameViewModel(videoId, randomizedFrames[0], _datasetServicesManager);
            SecondFrame = new FrameViewModel(videoId, randomizedFrames[1], _datasetServicesManager);
        }

    }
}
