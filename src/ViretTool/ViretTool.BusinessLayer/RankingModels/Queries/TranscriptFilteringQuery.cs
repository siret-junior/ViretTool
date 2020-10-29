namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class TranscriptFilteringQuery
    {
        public string VideoTranscriptQuery { get; set; }
        
        public TranscriptFilteringQuery(string videoTranscriptQuery)
        {
            VideoTranscriptQuery = videoTranscriptQuery;
        }

        public bool IsEmpty()
        {
            return VideoTranscriptQuery == null
                || VideoTranscriptQuery.Equals("");
        }
    }
}
