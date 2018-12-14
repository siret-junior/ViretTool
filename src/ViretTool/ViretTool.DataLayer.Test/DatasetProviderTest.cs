using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViretTool.DataLayer.DataModel;
using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataProviders.Dataset;
using ViretTool.DataLayer.DataIO.DatasetIO;

namespace ViretTool.DataLayer.Test
{
    [TestClass]
    public class DatasetProviderTest
    {
        private const string DATASET_TEST_FILE = "DatasetTestFile.dataset";

        [TestMethod]
        public void BinarySerializationTest()
        {
            Dataset datasetFromFilelist = DeserializeFromFilelist();

            DatasetProvider datasetProvider = new DatasetProvider();
            datasetProvider.ToBinaryFile(datasetFromFilelist, DATASET_TEST_FILE);
            Dataset datasetFromBinaryFile = datasetProvider.FromBinaryFile(DATASET_TEST_FILE);
            File.Delete(DATASET_TEST_FILE);

            byte[] serializedFromFilelist = Serialize(datasetFromFilelist);
            byte[] serializedFromBinaryFile = Serialize(datasetFromBinaryFile);

            Assert.IsTrue(serializedFromFilelist.SequenceEqual(serializedFromBinaryFile));
        }


        private static Dataset DeserializeFromFilelist()
        {
            Dataset datasetFromFilelist;
            byte[] byteArray = Encoding.UTF8.GetBytes(DatasetIOTestData.TEST_FILELIST);
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            using (StreamReader inputStream = new StreamReader(memoryStream))
            {
                DatasetFilelistFormatter formatter = new DatasetFilelistFormatter();
                datasetFromFilelist = formatter.Deserialize(inputStream, "Test dataset");
            }

            return datasetFromFilelist;
        }
        
        private static byte[] Serialize(Dataset datasetFromFilelist)
        {
            byte[] serialized;
            using (MemoryStream inputStream = new MemoryStream())
            {
                DatasetBinaryFormatter.Instance.Serialize(inputStream, datasetFromFilelist);
                serialized = inputStream.ToArray();
            }

            return serialized;
        }

        private static Dataset Deserialize(byte[] serialized)
        {
            Dataset deserialized;
            using (MemoryStream outputStream = new MemoryStream(serialized))
            {
                deserialized = DatasetBinaryFormatter.Instance.Deserialize(outputStream);
            }

            return deserialized;
        }

    }
}
