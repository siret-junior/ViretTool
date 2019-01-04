using System.Collections.Generic;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public interface IInteractionLogger
    {
        void LogInteraction(string category, string type = null, string value = null, string attributes = null);
        void ResetLog();
        int MemberId { get; set; }
        int TeamId { get; set; }
        string TeamName { get; set; }
        IReadOnlyList<Event> Events { get; }
    }
}
