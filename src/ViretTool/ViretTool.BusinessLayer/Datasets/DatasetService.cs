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
        private readonly Lazy<IReadOnlyDictionary<int, int[]>> _frameIdsForVideoId;

        public DatasetService(DatasetProvider datasetProvider, string datasetDirectory)
        {
            _dataset = datasetProvider.FromDirectory(datasetDirectory);
            _frameIdsForVideoId = new Lazy<IReadOnlyDictionary<int, int[]>>(() => _dataset.Videos.ToDictionary(v => v.Id, v => v.Frames.Select(f => f.Id).ToArray()));
        }

        public int[] VideoIds => _dataset.Videos.Select(v => v.Id).ToArray();

        public int[] GetFrameIdsForVideo(int videoId)
        {
            return _frameIdsForVideoId.Value[videoId];
        }
    }
}
