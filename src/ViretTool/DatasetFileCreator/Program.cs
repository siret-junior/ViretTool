using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataModel;
using ViretTool.DataLayer.DataProviders.Dataset;

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

            DatasetProvider datasetProvider = new DatasetProvider();
            Dataset dataset = datasetProvider.FromFilelist(inputFile, datasetName);
            datasetProvider.ToBinaryFile(dataset, outputFile);
        }
    }
}
