namespace ViretTool.BusinessLayer.ActionLogging
{
    public class Action
    {
        public Action(long timeStamp, LogCategory category, LogType type, string value = null, string attributes = null)
        {
            TimeStamp = timeStamp;
            Category = category;
            Type = type;
            Value = value;
            Attributes = attributes;
        }

        public string Attributes { get; }
        public LogCategory Category { get; }
        public long TimeStamp { get; }
        public LogType Type { get; }
        public string Value { get; }
    }
}
