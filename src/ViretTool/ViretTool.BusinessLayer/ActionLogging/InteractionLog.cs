using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLog
    {
        public InteractionLog()
        {
            Events = new List<Action>();
        }

        public string TeamName { get; set; } = "VIRET";
        public int TeamId { get; set; } = 4;
        public int MemberId { get; set; } = 1;

        public long TimeStamp { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public SubmissionType Type { get; set; }

        public List<Action> Events { get; }
        
        
    }
}
