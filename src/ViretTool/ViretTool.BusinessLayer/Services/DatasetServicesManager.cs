using System;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Services
{
    public class DatasetServicesManager : IDatasetServicesManager
    {
        private readonly IDatabaseServicesFactory _databaseServicesFactory;

        public DatasetServicesManager(IDatabaseServicesFactory databaseServicesFactory)
        {
            _databaseServicesFactory = databaseServicesFactory;
        }

        public DatasetServices CurrentDataset { get; private set; }
        public string CurrentDatasetFolder { get; private set; }
        public event EventHandler<DatasetServices> DatasetOpened;
        public event EventHandler DatasetReleased;

        public bool IsDatasetOpened => CurrentDataset != null;

        public void OpenDataset(string datasetDirectory)
        {
            if (IsDatasetOpened)
            {
                ReleaseDataset();
            }

            DatasetServices services = _databaseServicesFactory.Create(datasetDirectory);
            CurrentDataset = services;
            CurrentDatasetFolder = datasetDirectory;
            DatasetOpened?.Invoke(this, CurrentDataset);
        }

        public async Task OpenDatasetAsync(string datasetDirectory)
        {
            if (IsDatasetOpened)
            {
                ReleaseDataset();
            }

            DatasetServices services = await Task.Run(() => _databaseServicesFactory.Create(datasetDirectory));
            CurrentDataset = services;
            CurrentDatasetFolder = datasetDirectory;
            //event should be invoked on the main thread
            DatasetOpened?.Invoke(this, CurrentDataset);
        }

        private void ReleaseDataset()
        {
            _databaseServicesFactory.Destroy(CurrentDataset);
            CurrentDataset = null;
            CurrentDatasetFolder = null;
            DatasetReleased?.Invoke(this, EventArgs.Empty);
        }
    }
}
