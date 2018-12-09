using System;

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

        public void OpenDataset(string datasetFolder)
        {
            if (IsDatasetOpened)
            {
                ReleaseDataset();
            }

            DatasetServices services = _databaseServicesFactory.Create(datasetFolder);
            CurrentDataset = services;
            CurrentDatasetFolder = datasetFolder;
            DatasetOpened?.Invoke(this, CurrentDataset);
        }

        private void ReleaseDataset()
        {
            _databaseServicesFactory.Release(CurrentDataset);
            CurrentDataset = null;
            CurrentDatasetFolder = null;
            DatasetReleased?.Invoke(this, EventArgs.Empty);
        }
    }
}
