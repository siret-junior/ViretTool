namespace ViretTool.DataLayer.DataModel.Attributes
{
    public class DatasetAttributes
    {
        public readonly Dataset ParentDataset;

        public string DatasetName { get; private set; }

        public int VideoCount => ParentDataset.Videos.Count;
        public int ShotCount => ParentDataset.Shots.Count;
        public int GroupCount => ParentDataset.Groups.Count;
        public int FrameCount => ParentDataset.Frames.Count;

        public DatasetAttributes(Dataset parentDataset)
        {
            ParentDataset = parentDataset;
        }


        public override string ToString()
        {
            return DatasetName;
        }


        internal DatasetAttributes WithDatasetName(string fileName)
        {
            DatasetName = fileName;
            return this;
        }
    }
}
