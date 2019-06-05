using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ViretTool.BusinessLayer.Datasets;
using ViretTool.BusinessLayer.Descriptors;
using ViretTool.BusinessLayer.Services;
using ViretTool.DataLayer.DataIO.DatasetIO;
using ViretTool.DataLayer.DataModel;
using ViretTool.PresentationLayer.Controls.Common;

namespace ShotsDetector.PresentationLayer
{
    public class ShotViewModel : PropertyChangedBase
    {
        public ShotViewModel(IEnumerable<FrameViewModel> frames, int videoId)
        {
            VideoId = videoId;
            FramesForShot.AddRange(frames);
        }

        public int VideoId { get; }
        public BindableCollection<FrameViewModel> FramesForShot { get; } = new BindableCollection<FrameViewModel>();
    }

    public class MainWindowViewModel : Screen
    {
        private readonly IDatasetServicesManager _datasetServicesManager;

        private double _threshold = 0.3;
        private bool _isBusy;
        private string _datasetDirectory;

        public MainWindowViewModel(IDatasetServicesManager datasetServicesManager)
        {
            _datasetServicesManager = datasetServicesManager;
            _datasetDirectory = ConfigurationManager.AppSettings["databaseDirectory"];
        }

        public double Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                NotifyOfPropertyChange();
            }
        }

        public string DatasetDirectory
        {
            get => _datasetDirectory;
            set
            {
                _datasetDirectory = value;
                NotifyOfPropertyChange();
            }
        }


        public BindableCollection<ShotViewModel> Shots { get; } = new BindableCollection<ShotViewModel>();

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                NotifyOfPropertyChange();
            }
        }

        public async void GenerateShots()
        {
            await GenerateShotsInternal(
                (ints, i) => ((SemanticVectorDescriptorProvider)_datasetServicesManager.CurrentDataset.SemanticVectorProvider).GetDistance(ints.First(), i));
        }

        public async void GenerateShotsCentroids()
        {
            await GenerateShotsInternal(ComputeDistanceWithCluster);
        }

        private async Task GenerateShotsInternal(Func<IList<int>, int, double> distanceSelector, int videoCount = 10)
        {
            IsBusy = true;

            if (!_datasetServicesManager.IsDatasetOpened)
            {
                await _datasetServicesManager.OpenDatasetAsync(DatasetDirectory);
            }


            IList<ShotViewModel> shots = null;
            await Task.WhenAll(Task.Run(() => shots = Enumerable.Range(0, videoCount).SelectMany(videoId => ClusterVideoFrames(videoId, distanceSelector)).ToList()), Task.Delay(200));
            Shots.Clear();
            Shots.AddRange(shots);

            IsBusy = false;
        }

        public async void SaveShots()
        {
            IsBusy = true;

            await Task.Run(
                async () =>
                {
                    var datasetFile = Directory.GetFiles(DatasetDirectory, "*.dataset").Single();
                    File.Copy(datasetFile, datasetFile + "_", true);

                    var dataset = _datasetServicesManager.CurrentDataset.DatasetService.Dataset;
                    var shotId = 0;
                    var builder = new DatasetBuilder(dataset);
                    builder.ClearShots();

                    await GenerateShotsInternal(ComputeDistanceWithCluster, dataset.Videos.Count);

                    foreach (var shotForVideo in Shots.GroupBy(s => s.VideoId))
                    {
                        var shots = new List<Shot>();
                        foreach (var shot in shotForVideo)
                        {
                            shots.Add(builder.AddShot(shotId++, shot.FramesForShot.Select(f => dataset.Frames[GetFrameId(f).Value]).ToArray()));
                        }

                        builder.UpdateVideoShotMapping(dataset.Videos[shotForVideo.Key], shots.ToArray());
                    }

                    var newDataset = builder.Build();
                    using (var stream = File.Create(datasetFile))
                    {
                        DatasetBinarySerializer.Serialize(stream, newDataset);
                    }
                });

            MessageBox.Show("Done!");
            IsBusy = false;
        }

        private IEnumerable<ShotViewModel> ClusterVideoFrames(int videoId, Func<IList<int>, int, double> distanceSelector)
        {
            var frameIds = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdsForVideo(videoId);
            var shot = new List<int>();

            foreach (var id in frameIds)
            {
                if (!shot.Any())
                {
                    shot.Add(id);
                    continue;
                }

                var distance = distanceSelector(shot, id);
                if (distance > Threshold)
                {
                    yield return new ShotViewModel(shot.Select(GetFrameViewModelForFrameId), videoId);
                    shot.Clear();
                }

                shot.Add(id);
            }

            if (shot.Any())
            {
                yield return new ShotViewModel(shot.Select(GetFrameViewModelForFrameId), videoId);
            }
        }

        private double ComputeDistanceWithCluster(IList<int> ids, int newId)
        {
            double result = 0.0;
            var newDescriptor = _datasetServicesManager.CurrentDataset.SemanticVectorProvider[newId];

            for (var i = 0; i < newDescriptor.Length; i++)
            {
                var x = 0d;
                foreach (var id in ids)
                {
                    x += _datasetServicesManager.CurrentDataset.SemanticVectorProvider[id][i];
                }

                result += x / ids.Count * newDescriptor[i];
            }

            return 1 - result;
        }

        private FrameViewModel GetFrameViewModelForFrameId(int frameId)
        {
            IDatasetService datasetService = _datasetServicesManager.CurrentDataset.DatasetService;
            int videoId = datasetService.GetVideoIdForFrameId(frameId);
            int frameNumber = datasetService.GetFrameNumberForFrameId(frameId);
            return ConvertThumbnailToViewModel(videoId, frameNumber);
        }

        private FrameViewModel ConvertThumbnailToViewModel(int videoId, int frameNumber)
        {
            return new FrameViewModel(videoId, frameNumber, _datasetServicesManager);
        }

        protected int? GetFrameId(FrameViewModel frame)
        {
            return !_datasetServicesManager.CurrentDataset.DatasetService.TryGetFrameIdForFrameNumber(frame.VideoId, frame.FrameNumber, out int frameId) ? (int?)null : frameId;
        }
    }

}
