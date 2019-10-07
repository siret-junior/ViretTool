using System.Collections.Generic;
using System.Linq;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.DataLayer.DataIO.DatasetIO
{
    public class DatasetBuilder
    {
        private readonly Dataset _dataset;
        private readonly List<Shot> _shots;

        public DatasetBuilder(Dataset dataset)
        {
            _dataset = dataset;
            _shots = dataset.Shots.ToList();
        }

        public Dataset Build()
        {
            return new Dataset(_dataset.DatasetId, _dataset.Videos.ToArray(), _shots.ToArray(), /*_dataset.Groups.ToArray(), */_dataset.Frames.ToArray());
        }

        public void ClearShots()
        {
            _shots.Clear();
        }

        public Shot AddShot(int shotId, Frame[] framesInShot)
        {
            Shot newShot = new Shot(shotId);
            newShot.SetFrameMappings(framesInShot);
            _shots.Add(newShot);
            return newShot;
        }

        public void UpdateVideoShotMapping(Video video, Shot[] shots)
        {
            video.SetShotMappings(shots);
        }
    }
}
