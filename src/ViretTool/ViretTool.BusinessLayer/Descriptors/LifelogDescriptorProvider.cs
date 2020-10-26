using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.Services;
using ViretTool.DataLayer.DataIO.LifelogIO;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class LifelogDescriptorProvider : ILifelogDescriptorProvider
    {
        private Dictionary<(int videoId, int frameNumber), string> _filenameMapping;

        public LifelogDescriptorProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            Descriptors = datasetParameters.IsLifelogData
                              ? new LifelogDataReader().Read(Path.Combine(datasetDirectory, datasetParameters.LifelogDataFileName)).Select(Convert).ToArray()
                              : new LifelogFrameMetadata[0];

            _filenameMapping = LoadFilenameMapping(Directory.GetFiles(datasetDirectory).Where(fileName => fileName.EndsWith("filenames.txt")).First());

            //File.WriteAllLines("debug2.txt", _filenameMapping.Keys.Select(key => $"V:{key.videoId}, F:{key.frameNumber}, N:{_filenameMapping[key]}"));
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

        public string GetFilenameForFrame(int videoId, int frameNumber)
        {
            return _filenameMapping[(videoId, frameNumber)];
        }

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

        private Dictionary<(int videoId, int frameNumber), string> LoadFilenameMapping(string mappingFile)
        {
            Dictionary<(int videoId, int frameNumber), string> dictionary = new Dictionary<(int videoId, int frameNumber), string>();

            int iVideo = 0;
            int iFrameNumber = 0;
            string lastDate = null;
            foreach (string line in File.ReadAllLines(mappingFile))
            {
                string[] tokens = line.Split(new char[] { '/', '.' });
                string date = tokens[0];
                string filename = tokens[1];
                if (lastDate == null) lastDate = date;

                if (!lastDate.Equals(date))
                {
                    iVideo++;
                    iFrameNumber = 0;
                    lastDate = date;
                }

                dictionary.Add((iVideo, iFrameNumber), filename);
                iFrameNumber++;
            }

            return dictionary;
        }

    }
}
