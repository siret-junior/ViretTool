using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.DatasetIO;

namespace ViretTool.DataLayer.DataModel
{
    /// <summary>
    /// Loads the Dataset structure from a file.
    /// </summary>
    public class DatasetProvider
    {
        public const string FILE_EXTENSION = ".dataset";

        public static Dataset FromDirectory(string inputDirectory)
        {
            string[] files = Directory.GetFiles(inputDirectory);
            string inputFile = files.Where(path => path.EndsWith(FILE_EXTENSION)).Single();
            return FromBinaryFile(inputFile);
        }

        public static Dataset FromBinaryFile(string inputFilePath)
        {
            using (FileStream inputStream = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Dataset dataset = DatasetBinaryFormatter.Instance.Deserialize(inputStream);
                return dataset;
            }
            
//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }

        public static void ToBinaryFile(Dataset dataset, string outputFilePath)
        {
            using (FileStream outputStream = File.Open(outputFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                DatasetBinaryFormatter.Instance.Serialize(outputStream, dataset);
            }

//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }


        public static Dataset FromFilelist(string inputFilelistPath, string datasetName)
        {
            using (StreamReader inputStream = new StreamReader(inputFilelistPath))
            {
                DatasetFilelistFormatter formatter = new DatasetFilelistFormatter();
                Dataset dataset = formatter.Deserialize(inputStream, datasetName);
                return dataset;
            }

//#if DEBUG
//          TestDataset(Videos, Groups, Frames);
//#endif
        }
    }
}
