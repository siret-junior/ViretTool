using System;
using System.IO;
using System.Linq;
using System.Text;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.Services;
using ViretTool.DataLayer.DataIO.LifelogIO;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class LifelogDescriptorProvider : IDescriptorProvider<LifelogFrameMetadata>
    {
        public LifelogDescriptorProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            Descriptors = datasetParameters.IsLifelogData
                              ? new LifelogDataReader().Read(Path.Combine(datasetDirectory, datasetParameters.LifelogDataFileName)).Select(Convert).ToArray()
                              : new LifelogFrameMetadata[0];
        }

        public byte[] DatasetHeader => Encoding.UTF8.GetBytes("Lifelog data");
        public int DescriptorCount => Descriptors.Length;
        public int DescriptorLength => 1;
        public LifelogFrameMetadata[] Descriptors { get; }
        public LifelogFrameMetadata GetDescriptor(int index)
        {
            return Descriptors[index];
        }

        public LifelogFrameMetadata this[int index] => Descriptors[index];

        private LifelogFrameMetadata Convert(LifelogFrameInfo lifelogFrameInfo)
        {
            return new LifelogFrameMetadata(
                lifelogFrameInfo.FileName,
                lifelogFrameInfo.FromVideo,
                lifelogFrameInfo.DayOfWeek,
                lifelogFrameInfo.Time,
                lifelogFrameInfo.HeartRate,
                lifelogFrameInfo.GpsLocation,
                lifelogFrameInfo.GpsLatitude,
                lifelogFrameInfo.GpsLongitude);
        }
    }
}
