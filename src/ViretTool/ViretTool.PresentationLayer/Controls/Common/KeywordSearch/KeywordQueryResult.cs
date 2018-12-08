using System.Collections.Generic;

namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch
{
    public class KeywordQueryResult
    {
        public KeywordQueryResult(List<List<int>> query, string annotationSource)
        {
            Query = query;
            AnnotationSource = annotationSource;
        }

        public List<List<int>> Query { get; }
        public string AnnotationSource { get; }
    }
}
