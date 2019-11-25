using System.IO;
using System.Linq;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    /// <summary>
    /// Loads the Dataset structure from a file or directory based on a predefined file extension.
    /// </summary>
    public class DatasetProvider
    {
        public const string DATASET_FILE_EXTENSION = ".dataset";

        public DataModel.Dataset FromDirectory(string inputDirectory)
        {
            string[] files = Directory.GetFiles(inputDirectory);
            string datasetFile = files.Single(path => path.EndsWith(DATASET_FILE_EXTENSION));
            string datasetName = Path.GetFileNameWithoutExtension(datasetFile);
            return FromFilelist(datasetFile);
        }


        public DataModel.Dataset FromFilelist(string inputFilelistPath)
        {
            return new DatasetReader().ReadDataset(inputFilelistPath);
        }
    }
}
