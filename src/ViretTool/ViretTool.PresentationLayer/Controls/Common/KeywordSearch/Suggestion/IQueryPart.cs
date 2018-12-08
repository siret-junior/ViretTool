namespace ViretTool.PresentationLayer.Controls.Common.KeywordSearch.Suggestion
{
    public interface IQueryPart
    {
        int Id { get; }
        TextBlockType Type { get; }
        bool UseChildren { get; }
    }

    public enum TextBlockType
    {
        Class,
        OR,
        AND
    }
}
