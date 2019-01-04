using System.Collections.Generic;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLog
    {
        public InteractionLog()
        {
            Events = new List<Event>();
        }

        public List<Event> Events { get; }
        public int MemberId { get; set; } = 1;
        public int TeamId { get; set; } = 4;

        public string TeamName { get; set; } = "VIRET";
    }
}
