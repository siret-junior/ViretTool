using System.Collections.Generic;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch
{
    public class KeywordQueryResult
    {
        public KeywordQueryResult(List<List<int>> query, string fullQuery, string annotationSource)
        {
            Query = query;
            FullQuery = fullQuery;
            AnnotationSource = annotationSource;
        }

        public List<List<int>> Query { get; }
        public string FullQuery { get; }
        public string AnnotationSource { get; }
    }
}
