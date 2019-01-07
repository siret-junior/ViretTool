using System;
using System.Linq;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLogger : IInteractionLogger
    {
        private const int TimeDelayMiliseconds = 1000;

        private readonly object _lockObject = new object();

		public InteractionLog Log { get; } = new InteractionLog();

        public void LogInteraction(LogCategory category, LogType type, string value = null, string attributes = null)
        {
            // TODO: considering events with only a single action for now
            lock (_lockObject)
            {
                long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (Log.Events.Count > 0 && Log.Events.Last().Category == category && Log.Events.Last().Type == type)
                {
                    long lastEventTime = Log.Events[Log.Events.Count - 1].TimeStamp;
                    if (Math.Abs(lastEventTime - currentTime) < TimeDelayMiliseconds)
                    {
                        return;
                    }
                }

                Action action = new Action(currentTime, category, type, value, attributes);
                Log.Events.Add(action);
            }
        }

        public void ResetLog()
        {
            lock (_lockObject)
            {
                Log.Events.Clear();
            }
        }
    }
}
