using System.IO;
using System.Threading;
using ViretTool.DataLayer.DataIO.DatasetIO;
using ViretTool.DataLayer.DataModel;

namespace DatasetFileCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            string datasetName = args[0];
            string inputFile = Path.GetFullPath(args[1]);
            string outputFile = Path.GetFullPath(args[2]);
            bool isLSC = false;
            if (args.Length > 3 && args[3].Equals("LSC")) isLSC = true;

            Dataset dataset;
            if (isLSC)
            {
                DatasetFilelistDeserializerLSC datasetDeserializer = new DatasetFilelistDeserializerLSC();
                dataset = datasetDeserializer.Deserialize(new StreamReader(inputFile), datasetName);
            }
            else
            {
                DatasetFilelistDeserializer datasetDeserializer = new DatasetFilelistDeserializer();
                dataset = datasetDeserializer.Deserialize(new StreamReader(inputFile), datasetName);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            DatasetBinarySerializer.Serialize(File.OpenWrite(outputFile), dataset);
        }
    }
}
