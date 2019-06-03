using System;
using System.IO;
using System.Linq;

namespace ViretTool.BusinessLayer.Services
{
    public interface IDatasetParameters
    {
        bool IsLifelogData { get; }
        string LifelogDataFileName { get; }

        bool IsInitialDisplayPrecomputed { get; }
        string InitialDisplayFileName { get; }
    }

    public class DatasetParameters : IDatasetParameters
    {
        public DatasetParameters(string datasetDirectory)
        {
            string[] datasetFiles = Directory.GetFiles(datasetDirectory);
            IsLifelogData = datasetFiles.Any(f => Path.GetFileName(f).Equals(LifelogDataFileName, StringComparison.CurrentCultureIgnoreCase));
            IsInitialDisplayPrecomputed = datasetFiles.Any(f => Path.GetFileName(f).Equals(InitialDisplayFileName, StringComparison.CurrentCultureIgnoreCase));
        }

        public string LifelogDataFileName => "lifelog-data.json";
        
        public bool IsLifelogData { get; }


        public string InitialDisplayFileName => "initial-display-ids.txt";
        public bool IsInitialDisplayPrecomputed { get; }
    }
}
