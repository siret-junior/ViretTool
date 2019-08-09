using System.Collections.Generic;
using ViretTool.BusinessLayer.RankingModels.Queries;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch
{
    public class KeywordQueryResult
    {
        public KeywordQueryResult(SynsetClause[] query, string fullQuery, string annotationSource)
        {
            Query = query;
            FullQuery = fullQuery;
            AnnotationSource = annotationSource;
        }

        public SynsetClause[] Query { get; }
        public string FullQuery { get; }
        public string AnnotationSource { get; }
    }
}
