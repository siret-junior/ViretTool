using System;
using System.Collections.Generic;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLogger : IInteractionLogger
    {
        private const int TimeDelayMiliseconds = 1000;

        private readonly object _lockObject = new object();
        private readonly List<Event> _events = new List<Event>();

        public int MemberId { get; set; } = 1;
        public int TeamId { get; set; } = 4;
        public string TeamName { get; set; } = "VIRET";

        public IReadOnlyList<Event> Events
        {
            get
            {
                lock (_lockObject)
                {
                    return _events;
                }
            }
        }

        public void LogInteraction(string category, string type = null, string value = null, string attributes = null)
        {
            // TODO: considering events with only a single action for now
            Event interactionEvent = new Event();
            Action action = new Action(category, type, value, attributes);
            interactionEvent.Actions.Add(action);

            lock (_lockObject)
            {
                if (_events.Count > 0)
                {
                    long lastEventTime = _events[_events.Count - 1].Timestamp;
                    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if (Math.Abs(lastEventTime - currentTime) < TimeDelayMiliseconds)
                    {
                        return;
                    }
                }

                _events.Add(interactionEvent);
            }
        }

        public void ResetLog()
        {
            lock (_lockObject)
            {
                _events.Clear();
            }
        }
    }
}
