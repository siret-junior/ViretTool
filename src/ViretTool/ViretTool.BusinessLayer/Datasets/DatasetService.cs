using System;
using System.Collections.Generic;
using System.Linq;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ViretTool.BusinessLayer.Datasets
{
    public class DatasetService : IDatasetService
    {
        private readonly Dataset _dataset;
        private readonly Lazy<IReadOnlyDictionary<(int, int), int>> _frameIdForFrameNumber;
        private readonly Lazy<IReadOnlyDictionary<int, int[]>> _frameIdsForVideoId;
        private readonly Lazy<IReadOnlyDictionary<int, int>> _frameNumberForFrameId;
        private readonly Lazy<IReadOnlyDictionary<int, int[]>> _frameNumbersForVideoId;
        private readonly Lazy<IReadOnlyDictionary<int, int>> _videoIdForFrameId;

        public DatasetService(DatasetProvider datasetProvider, string datasetDirectory)
        {
            _dataset = datasetProvider.FromDirectory(datasetDirectory);
            _frameIdForFrameNumber = new Lazy<IReadOnlyDictionary<(int, int), int>>(() => _dataset.Frames.ToDictionary(f => (f.ParentVideo.Id, f.FrameNumber), f => f.Id));
            _frameIdsForVideoId = new Lazy<IReadOnlyDictionary<int, int[]>>(() => _dataset.Videos.ToDictionary(v => v.Id, v => v.Frames.Select(f => f.Id).ToArray()));
            _frameNumberForFrameId = new Lazy<IReadOnlyDictionary<int, int>>(() => _dataset.Frames.ToDictionary(f => f.Id, f => f.FrameNumber));
            _frameNumbersForVideoId = new Lazy<IReadOnlyDictionary<int, int[]>>(() => _dataset.Videos.ToDictionary(v => v.Id, v => v.Frames.Select(f => f.FrameNumber).ToArray()));
            _videoIdForFrameId = new Lazy<IReadOnlyDictionary<int, int>>(() => _dataset.Frames.ToDictionary(f => f.Id, f => f.ParentVideo.Id));
        }

        public bool TryGetFrameIdForFrameNumber(int videoId, int frameNumber, out int frameId)
        {
            return _frameIdForFrameNumber.Value.TryGetValue((videoId, frameNumber), out frameId);
        }

        public int GetFrameIdForFrameNumber(int videoId, int frameNumber)
        {
            return _frameIdForFrameNumber.Value[(videoId, frameNumber)];
        }

        public int[] GetFrameIdsForVideo(int videoId)
        {
            return _frameIdsForVideoId.Value[videoId];
        }

        public int GetFrameNumberForFrameId(int frameId)
        {
            return _frameNumberForFrameId.Value[frameId];
        }

        public int[] GetFrameNumbersForVideo(int videoId)
        {
            return _frameNumbersForVideoId.Value[videoId];
        }

        public int GetVideoIdForFrameId(int frameId)
        {
            return _videoIdForFrameId.Value[frameId];
        }

        public int[] VideoIds => _dataset.Videos.Select(v => v.Id).ToArray();
    }
}
