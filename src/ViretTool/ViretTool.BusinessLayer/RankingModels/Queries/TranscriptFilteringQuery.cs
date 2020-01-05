namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class TranscriptFilteringQuery
    {
        public string VideoTranscriptQuery { get; set; }
        
        public TranscriptFilteringQuery(string videoTranscriptQuery)
        {
            VideoTranscriptQuery = videoTranscriptQuery;
        }
    }
}
