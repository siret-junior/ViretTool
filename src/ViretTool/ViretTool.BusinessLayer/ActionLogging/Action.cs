using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class Action
    {
        public Action(long timestamp, LogCategory category, LogType type, string value = null/*, string attributes = null*/)
        {
            Timestamp = timestamp;
            Category = category;
            Type = new LogType[] { type };
            Value = value;
            //Attributes = attributes;
        }

        public long Timestamp { get; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LogCategory Category { get; }

        //[JsonConverter(typeof(StringEnumConverter))]
        public LogType[] Type { get; }

        public string Value { get; }

        //public string Attributes { get; }
    }
}
