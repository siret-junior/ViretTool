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
        private readonly Lazy<IReadOnlyDictionary<int, (int StartFrame, int EndFrame)[]>> _shotFrameNumbersForVideoIds;
        private readonly Lazy<IReadOnlyDictionary<int, int>> _lastFrameIdForVideoId;
        private readonly Lazy<int[]> _lastFrameIdsInVideoForFrameId;
        private readonly Lazy<int[]> _firstFrameIdsInVideoForFrameId;

        public DatasetService(DatasetProvider datasetProvider, string datasetDirectory)
        {
            _dataset = datasetProvider.FromDirectory(datasetDirectory);
            //frames in videos contain only selected (not all 4FPS) frames
            _frameIdForFrameNumber = new Lazy<IReadOnlyDictionary<(int, int), int>>(() => _dataset.Frames.ToDictionary(f => (f.ParentVideo.Id, f.FrameNumber), f => f.Id));
            _frameIdsForVideoId = new Lazy<IReadOnlyDictionary<int, int[]>>(() => _dataset.Videos.ToDictionary(v => v.Id, v => v.Frames.Select(f => f.Id).ToArray()));
            _frameNumberForFrameId = new Lazy<IReadOnlyDictionary<int, int>>(() => _dataset.Frames.ToDictionary(f => f.Id, f => f.FrameNumber));
            _frameNumbersForVideoId = new Lazy<IReadOnlyDictionary<int, int[]>>(() => _dataset.Videos.ToDictionary(v => v.Id, v => v.Frames.Select(f => f.FrameNumber).ToArray()));
            _videoIdForFrameId = new Lazy<IReadOnlyDictionary<int, int>>(() => _dataset.Frames.ToDictionary(f => f.Id, f => f.ParentVideo.Id));
            _shotFrameNumbersForVideoIds = new Lazy<IReadOnlyDictionary<int, (int, int)[]>>(
                () => _dataset.Videos.ToDictionary(v => v.Id, v => v.Shots.Select(s => (s.StartFrameNumber, s.EndFrameNumber)).ToArray()));
            _lastFrameIdForVideoId = new Lazy<IReadOnlyDictionary<int, int>>(() 
                => _dataset.Videos.ToDictionary(video => video.Id, video => video.Frames.Select(f => f.Id).Last()));
            _lastFrameIdsInVideoForFrameId = new Lazy<int[]>(()
                => _dataset.Frames.Select(frame => frame.ParentVideo.Frames.Select(f => f.Id).Last()).ToArray());
            _firstFrameIdsInVideoForFrameId = new Lazy<int[]>(()
                => _dataset.Frames.Select(frame => frame.ParentVideo.Frames.Select(f => f.Id).First()).ToArray());
        }

        public bool TryGetFrameIdForFrameNumber(int videoId, int frameNumber, out int frameId) => _frameIdForFrameNumber.Value.TryGetValue((videoId, frameNumber), out frameId);

        public int GetFrameIdForFrameNumber(int videoId, int frameNumber) => _frameIdForFrameNumber.Value[(videoId, frameNumber)];

        public int[] GetFrameIdsForVideo(int videoId) => _frameIdsForVideoId.Value[videoId];

        public int GetFrameNumberForFrameId(int frameId) => _frameNumberForFrameId.Value[frameId];

        public int[] GetFrameNumbersForVideo(int videoId) => _frameNumbersForVideoId.Value[videoId];

        public int GetVideoIdForFrameId(int frameId) => _videoIdForFrameId.Value[frameId];

        public (int StartFrame, int EndFrame)[] GetShotFrameNumbersForVideo(int videoId) => _shotFrameNumbersForVideoIds.Value[videoId];

        public int[] VideoIds => _dataset.Videos.Select(v => v.Id).ToArray();

        public int FrameCount => _dataset.Frames.Count;

        public int GetLastFrameIdForVideo(int videoId) => _lastFrameIdForVideoId.Value[videoId];
        public int[] GetLastFrameIdsInVideoForFrameId() => _lastFrameIdsInVideoForFrameId.Value;
        public int[] GetFirstFrameIdsInVideoForFrameId() => _firstFrameIdsInVideoForFrameId.Value;
    }
}
