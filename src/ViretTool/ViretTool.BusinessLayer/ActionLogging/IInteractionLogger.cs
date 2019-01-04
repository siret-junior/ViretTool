using System.Collections.Generic;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public interface IInteractionLogger
    {
        void LogInteraction(string category, string type = null, string value = null, string attributes = null);
        void ResetLog();
        InteractionLog Log { get; }
    }
}
