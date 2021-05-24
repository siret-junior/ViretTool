using System;
using System.Threading.Tasks;
using Viret;

namespace ViretTool.BusinessLayer.Services
{
    public interface IDatasetServicesManager
    {
        ViretCore ViretCore { get; }
        DatasetServices CurrentDataset { get; }
        string CurrentDatasetFolder { get; }
        bool IsDatasetOpened { get; }

        event EventHandler<DatasetServices> DatasetOpened;
        event EventHandler DatasetReleased;

        void OpenDataset(string datasetFolder);
        Task OpenDatasetAsync(string datasetDirectory);
    }
}
