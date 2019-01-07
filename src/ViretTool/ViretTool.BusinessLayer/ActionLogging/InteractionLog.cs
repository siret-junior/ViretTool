using System.Collections.Generic;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLog
    {
        public InteractionLog()
        {
            Events = new List<Action>();
        }

        public List<Action> Events { get; }
        public int MemberId { get; set; } = 1;
        public int TeamId { get; set; } = 4;

        public string TeamName { get; set; } = "VIRET";
    }
}
