namespace ViretTool.BusinessLayer.RankingModels
{
    public struct RankedFrame
    {
        public int Id { get; }
        public float Rank { get; }


        public RankedFrame(int id, float rank)
        {
            Id = id;
            Rank = rank;
        }
    }
}
