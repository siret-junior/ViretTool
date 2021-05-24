using System.Collections.Generic;
//using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch
{
    public class KeywordQueryResult
    {
        public KeywordQueryResult(string[] query, string fullQuery, string annotationSource)
        {
            Query = query;
            FullQuery = fullQuery;
            AnnotationSource = annotationSource;
        }

        public string[] Query { get; }
        public string FullQuery { get; }
        public string AnnotationSource { get; }
    }
}
