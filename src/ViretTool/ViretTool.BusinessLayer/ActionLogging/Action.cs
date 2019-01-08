using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        public long TimeStamp { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogCategory Category { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogType Type { get; }

        public string Value { get; }

        public string Attributes { get; }
    }
}
