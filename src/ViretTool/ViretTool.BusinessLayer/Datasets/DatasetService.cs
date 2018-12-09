using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

namespace ViretTool.BusinessLayer.Datasets
{
    public class DatasetService : IDatasetService
    {
        private readonly Dataset _dataset;

        public DatasetService(DatasetProvider datasetProvider, string datasetFolder)
        {
            _dataset = datasetProvider.FromDirectory(datasetFolder);
        }

        public int VideoCount => _dataset.Videos.Count;
    }
}
