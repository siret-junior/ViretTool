using System;
using System.IO;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    public class DatasetFilelistFormatter
    {
        public DatasetFilelistFormatter()
        {
        }


        public void Serialize(Stream serializationStream, Dataset dataset)
        {
            throw new NotImplementedException();
        }

        public Dataset Deserialize(StreamReader serializationStream, string datasetName)
        {
            // TODO: singleton
            return new DatasetReader().ReadDataset(serializationStream);
        }
    }
}
