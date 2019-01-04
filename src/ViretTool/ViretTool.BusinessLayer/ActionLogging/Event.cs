using System;
using System.Collections.Generic;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class Event
    {
        public Event()
        {
            Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Actions = new List<Action>();
        }

        public List<Action> Actions { get; }
        public long Timestamp { get; }
    }
}
