using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ViretTool.DataLayer.DataIO.DatasetIO;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.DataLayer.Test
{
    [TestClass]
    public class DatasetIOTest
    {
        [TestMethod]
        public void FilelistDeserializerTest()
        {
            Dataset datasetFromFilelist = DeserializeFromFilelist();

            byte[] serialized = Serialize(datasetFromFilelist);
            Dataset deserialized = Deserialize(serialized);
            byte[] serializedAgain = Serialize(deserialized);

            Assert.IsTrue(serialized.SequenceEqual(serializedAgain));
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


    }
}
