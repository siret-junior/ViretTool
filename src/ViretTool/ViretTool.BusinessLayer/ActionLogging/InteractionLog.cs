using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLog
    {
        public InteractionLog()
        {
            Events = new List<Action>();
            TeamId = int.Parse(ConfigurationManager.AppSettings["teamId"] ?? "7");
            MemberId = int.Parse(ConfigurationManager.AppSettings["memberId"] ?? "-1");
        }

        //public string TeamName { get; set; } = "VIRET";
        public int TeamId { get; set; }
        public int MemberId { get; set; }

        public long Timestamp { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public SubmissionType Type { get; set; }

        public List<Action> Events { get; }
        
        
    }
}
