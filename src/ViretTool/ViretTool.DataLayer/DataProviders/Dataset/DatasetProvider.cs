using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.DatasetIO;

namespace ViretTool.DataLayer.DataProviders.Dataset
{
    /// <summary>
    /// Loads the Dataset structure from a file.
    /// </summary>
    public class DatasetProvider
    {
        public const string FILE_EXTENSION = ".dataset";

        public DataModel.Dataset FromDirectory(string inputDirectory)
        {
            string[] files = Directory.GetFiles(inputDirectory);
            string inputFile = files.Single(path => path.EndsWith(FILE_EXTENSION));
            return FromBinaryFile(inputFile);
        }

        public DataModel.Dataset FromBinaryFile(string inputFilePath)
        {
            using (FileStream inputStream = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                DataModel.Dataset dataset = DatasetBinaryFormatter.Instance.Deserialize(inputStream);
                return dataset;
            }
            
//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }

        public void ToBinaryFile(DataModel.Dataset dataset, string outputFilePath)
        {
            using (FileStream outputStream = File.Open(outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                DatasetBinaryFormatter.Instance.Serialize(outputStream, dataset);
            }

//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }


        public DataModel.Dataset FromFilelist(string inputFilelistPath, string datasetName)
        {
            using (StreamReader inputStream = new StreamReader(inputFilelistPath))
            {
                DatasetFilelistFormatter formatter = new DatasetFilelistFormatter();
                DataModel.Dataset dataset = formatter.Deserialize(inputStream, datasetName);
                return dataset;
            }

//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }
    }
}
