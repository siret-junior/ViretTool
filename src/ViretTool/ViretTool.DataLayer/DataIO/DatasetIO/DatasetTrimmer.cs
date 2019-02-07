using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    public class DatasetTrimmer
    {
        public static Dataset Trim(string inputFile, string outputFile, int maxVideos)
        {
            Dataset inputDataset = DatasetBinaryFormatter.Instance.Deserialize(File.OpenRead(inputFile));
            Dataset trimmedDataset = Trim(inputDataset, maxVideos);
            DatasetBinaryFormatter.Instance.Serialize(
                File.Open(outputFile, FileMode.CreateNew, FileAccess.Write, FileShare.None), 
                trimmedDataset);
            return trimmedDataset;
        }

        public static Dataset Trim(Dataset inputDataset, int maxVideos)
        {
            byte[] datasetId = UpdateDatasetId(inputDataset, maxVideos);
            Video[] videos = TrimVideos(inputDataset, maxVideos);
            Shot[] shots = TrimShots(inputDataset, maxVideos);
            Group[] groups = TrimGroups(inputDataset, maxVideos);
            Frame[] frames = TrimFrames(inputDataset, maxVideos);
            
            return new Dataset(datasetId, videos, shots, groups, frames);
        }


        private static byte[] UpdateDatasetId(Dataset inputDataset, int maxVideos)
        {
            FileHeaderUtilities.DecodeDatasetID(inputDataset.DatasetId,
                            out string datasetName, out DateTime creationTime);
            byte[] datasetId = FileHeaderUtilities.EncodeDatasetID(
                datasetName + "_first" + maxVideos, DateTime.Now);
            return datasetId;
        }

        private static Video[] TrimVideos(Dataset inputDataset, int maxVideos)
        {
            return inputDataset.Videos.Take(maxVideos).ToArray();
        }

        private static Shot[] TrimShots(Dataset inputDataset, int maxVideos)
        {
            int shotCount = inputDataset.Videos[maxVideos - 1].Shots.Last().Id + 1;
            return inputDataset.Shots.Take(shotCount).ToArray();
        }

        private static Group[] TrimGroups(Dataset inputDataset, int maxVideos)
        {
            int groupCount = inputDataset.Videos[maxVideos - 1].Groups.Last().Id + 1;
            return inputDataset.Groups.Take(groupCount).ToArray();
        }

        private static Frame[] TrimFrames(Dataset inputDataset, int maxVideos)
        {
            int frameCount = inputDataset.Videos[maxVideos - 1].Frames.Last().Id + 1;
            return inputDataset.Frames.Take(frameCount).ToArray();
        }
    }
}
