using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Logging.DresApi
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventCategory
    {
        Text, Image, Sketch, Filter, Browsing, Cooperation, Other
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        // text
        JointEmbedding, LocalizedObject, Caption,
        // image
        GlobalFeatures,
        // sketch
        Color,
        // Filter
        BW, MaxFrames, Lifelog, ASR,
        // browsing
        RankedList, VideoSummary, TemporalContext, Exploration, ExplicitSort, ResetAll,
        // none
        None
    }


    public class QueryEvent
    {
        public long Timestamp;
        public EventCategory Category;
        public EventType Type;
        public string Value;

        public QueryEvent(long timestamp, EventCategory category, EventType type, string value)
        {
            Timestamp = timestamp;
            Category = category;
            Type = type;
            Value = value;
        }

        public QueryEvent(EventCategory category, EventType type, string value)
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Category = category;
            Type = type;
            Value = value;
        }
    }
}
