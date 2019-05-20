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
        public const string FRAME_ATTRIBUTES_EXTENSION = ".frameattributes";

        public DataModel.Dataset FromDirectory(string inputDirectory)
        {
            string[] files = Directory.GetFiles(inputDirectory);
            string datasetFile = files.Single(path => path.EndsWith(FILE_EXTENSION));
            string frameAttributesFile = files.Single(path => path.EndsWith(FRAME_ATTRIBUTES_EXTENSION));
            return FromBinaryFile(datasetFile, frameAttributesFile);
        }

        public DataModel.Dataset FromBinaryFile(string datasetPath, string frameAttributesPath = null)
        {
            try
            {
                using (FileStream inputStream = File.Open(datasetPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    DataModel.Dataset dataset = DatasetBinaryFormatter.Instance.Deserialize(inputStream);

                    // TODO:
                    if (frameAttributesPath != null)
                        using (StreamReader reader = new StreamReader(frameAttributesPath))
                        {
                            for (int i = 0; i < dataset.Frames.Count; i++)
                            {
                                dataset.Frames[i].FrameNumber = int.Parse(reader.ReadLine());
                            }
                        }


                    return dataset;
                }
            }
            catch
            {
                throw;
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
