using System;
using System.IO;
using System.Linq;
using Castle.Core.Logging;

namespace ViretTool.BusinessLayer.Services
{
    public interface IDatasetParameters
    {
        int DefaultFrameWidth { get; }
        int DefaultFrameHeight { get; }

        bool IsLifelogData { get; }
        string LifelogDataFileName { get; }

        bool IsInitialDisplayPrecomputed { get; }
        string InitialDisplayFileName { get; }
        
    }

    public class DatasetParameters : IDatasetParameters
    {
        private readonly ILogger _logger;

        public DatasetParameters(string datasetDirectory, ILogger logger)
        {
            _logger = logger;
            string[] datasetFiles = Directory.GetFiles(datasetDirectory);
            IsLifelogData = datasetFiles.Any(f => Path.GetFileName(f).Equals(LifelogDataFileName, StringComparison.CurrentCultureIgnoreCase));
            IsInitialDisplayPrecomputed = datasetFiles.Any(f => Path.GetFileName(f).Equals(InitialDisplayFileName, StringComparison.CurrentCultureIgnoreCase));

            (DefaultFrameWidth, DefaultFrameHeight) = LoadFrameMeasures(datasetDirectory);
        }

        public int DefaultFrameWidth { get; }
        public int DefaultFrameHeight { get; }

        public string LifelogDataFileName => "lifelog-data.json";
        
        public bool IsLifelogData { get; }


        public string InitialDisplayFileName => "initial-display-ids.txt";
        public bool IsInitialDisplayPrecomputed { get; }

        private (int DefaultFrameWidth, int DefaultFrameHeight) LoadFrameMeasures(string datasetDirectory)
        {
            try
            {
                int[] resolution = File.ReadAllText(Path.Combine(datasetDirectory, "frame-resolution.txt")).Split('x').Select(int.Parse).ToArray();
                return (resolution[0], resolution[1]);
            }
            catch (Exception e)
            {
                _logger.Info(e.Message, e);
                return (128, 72);
            }
        }
    }
}
