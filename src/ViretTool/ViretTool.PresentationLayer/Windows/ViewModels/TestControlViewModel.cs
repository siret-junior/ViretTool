using System;
using System.Linq;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Submission;
using ViretTool.PresentationLayer.Controls.Common;

namespace ViretTool.PresentationLayer.Windows.ViewModels
{
    public class TestControlViewModel : Screen
    {
        private readonly IDatasetServicesManager _datasetServicesManager;
        private readonly IQueryPersistingService _queryPersistingService;
        private readonly Random _random = new Random();

        public TestControlViewModel(IDatasetServicesManager datasetServicesManager, IQueryPersistingService queryPersistingService)
        {
            _datasetServicesManager = datasetServicesManager;
            _queryPersistingService = queryPersistingService;
            ImageHeight = int.Parse(Resources.Properties.Resources.ImageHeight) * 2;
            ImageWidth = int.Parse(Resources.Properties.Resources.ImageWidth) * 2;
        }

        public int ImageHeight { get; }

        public int ImageWidth { get; }

        public BindableCollection<FrameViewModel> Frames { get; } = new BindableCollection<FrameViewModel>();

        public void InitializeFramesRandomly()
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            int videoId = datasetService.VideoIds[_random.Next(datasetService.VideoIds.Length)];

            int[] frameNumbersForVideo = datasetService.GetFrameNumbersForVideo(videoId);
            int randomIndex = _random.Next(Math.Max(frameNumbersForVideo.Length - 5, 0));
            
            Frames.Clear();
            Frames.AddRange(frameNumbersForVideo.Skip(randomIndex).Take(5).Select(fn => new FrameViewModel(videoId, fn, _datasetServicesManager)));
            _queryPersistingService.SaveTestObjects(videoId, Frames.Select(f => f.FrameNumber).ToList());
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                _queryPersistingService.SaveTestEnd();
            }
        }
    }
}
