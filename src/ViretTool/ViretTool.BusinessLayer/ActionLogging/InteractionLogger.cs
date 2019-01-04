using System;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLogger : IInteractionLogger
    {
        private const int TimeDelayMiliseconds = 1000;

        private readonly object _lockObject = new object();

		public InteractionLog Log { get; } = new InteractionLog();

        public void LogInteraction(string category, string type = null, string value = null, string attributes = null)
        {
            // TODO: considering events with only a single action for now
            Event interactionEvent = new Event();
            Action action = new Action(category, type, value, attributes);
            interactionEvent.Actions.Add(action);

            lock (_lockObject)
            {
                if (Log.Events.Count > 0)
                {
                    long lastEventTime = Log.Events[Log.Events.Count - 1].Timestamp;
                    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (Math.Abs(lastEventTime - currentTime) < TimeDelayMiliseconds)
                    {
                        return;
                    }
                }

                Log.Events.Add(interactionEvent);
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
