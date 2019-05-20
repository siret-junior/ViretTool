using System.IO;
using System.Linq;

namespace ViretTool.BusinessLayer.Services
{
    public interface IDatasetParameters
    {
        bool LifelogFiltersVisible { get; }
        bool GpsFilterVisible { get; }
    }

    public class DatasetParameters : IDatasetParameters
    {
        public DatasetParameters(string datasetDirectory)
        {
            string[] datasetFiles = Directory.GetFiles(datasetDirectory);
            //TODO
            LifelogFiltersVisible = datasetFiles.Any(f => f.EndsWith(".hb") && f.EndsWith(".week")); //heartbeat and day of week
            GpsFilterVisible = datasetFiles.Any(f => f.EndsWith(".gps"));
        }

        public bool LifelogFiltersVisible { get; }
        public bool GpsFilterVisible { get; }
    }
}
