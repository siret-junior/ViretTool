using System;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Services
{
    public interface IDatasetServicesManager
    {
        DatasetServices CurrentDataset { get; }
        string CurrentDatasetFolder { get; }
        bool IsDatasetOpened { get; }

        event EventHandler<DatasetServices> DatasetOpened;
        event EventHandler DatasetReleased;

        void OpenDataset(string datasetFolder);
        Task OpenDatasetAsync(string datasetDirectory);
    }
}
