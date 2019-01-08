using System.Collections.Generic;

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

        public List<Action> Events { get; }
        
        
    }
}
