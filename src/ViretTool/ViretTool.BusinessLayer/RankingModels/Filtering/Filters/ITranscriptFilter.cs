namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public interface ITranscriptFilter
    {
        bool[] GetFilterMask(string query);
    }
}
