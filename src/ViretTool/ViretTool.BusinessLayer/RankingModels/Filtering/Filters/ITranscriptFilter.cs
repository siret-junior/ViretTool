namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public interface ITranscriptFilter
    {
        //bool[] FilterVideos(string query);
        bool[] GetFilterMask(string query);
    }
}
