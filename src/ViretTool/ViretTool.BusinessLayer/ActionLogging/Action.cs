namespace ViretTool.BusinessLayer.ActionLogging
{
    public class Action
    {
        public Action(string category, string type = null, string value = null, string attributes = null)
        {
            Category = category;
            Type = type;
            Value = value;
            Attributes = attributes;
        }

        public string Attributes { get; }
        public string Category { get; }
        public string Type { get; }
        public string Value { get; }
    }
}
