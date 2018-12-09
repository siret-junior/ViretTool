using System.Linq;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ViretTool.BusinessLayer.Datasets
{
    public class DatasetService : IDatasetService
    {
        private readonly Dataset _dataset;

        public DatasetService(DatasetProvider datasetProvider, string datasetDirectory)
        {
            _dataset = datasetProvider.FromDirectory(datasetDirectory);
        }

        public int[] VideoIds => _dataset.Videos.Select(v => v.Id).ToArray();

        public int[] GetFrameIdsForVideo(int videoId)
        {
            //TODO more efficiently, perhaps dictionary
            return _dataset.Videos.Single(v => v.Id == videoId).Frames.Select(frame => frame.Id).ToArray();
        }
    }
}
