using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viret.Logging.DresApi;
using Viret.Logging.Json;

namespace Viret.Logging
{
    
    /// <summary>
    /// Receives query changes, holds the current state of the query, logs it locally, provides the state for remote result logging.
    /// Receives browsing events, individually logs them locally, accummulates and provides them for remote result logging.
    /// Receives result set changes, submits result set on change, together with current state of the query and all accumulated browsing events.
    /// </summary>
    public class InteractionLogger : IDisposable
    {
        //public QueryEvent CurrentQuery = null;

        private const int EVENT_AGGREGATION_DELAY_MS = 1000;
        private List<QueryEvent> _browsingEvents = new List<QueryEvent>();
        private readonly object _browsingEventsLock = new object();
        private readonly StreamWriter _localLogger;

        public InteractionLogger()
        {
            string logDirectory = Path.Combine("Logs", "Interactions");
            Directory.CreateDirectory(logDirectory);
            string filename = $"InteractionLog_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _localLogger = new StreamWriter(Path.Combine(logDirectory, filename))
            {
                AutoFlush = true
            };
        }


        public void LogInteraction(EventCategory category, EventType type, string value = "")
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            QueryEvent queryEvent = new QueryEvent(timestamp, category, type, value);

            // store current query state
            //if (category == EventCategory.Text)
            //{
            //    CurrentQuery = queryEvent;
            //}

            // accumulate browsing events
            if (category == EventCategory.Browsing)
            {
                lock (_browsingEventsLock)
                {
                    // aggregate events occuring in quick succession (scrolling, etc.)
                    if (_browsingEvents.Count > 0
                        && _browsingEvents.Last().Category == category
                        && _browsingEvents.Last().Type == type)
                    {
                        long lastEventTime = _browsingEvents.Last().Timestamp;
                        if (Math.Abs(lastEventTime - timestamp) < EVENT_AGGREGATION_DELAY_MS)
                        {
                            return;
                        }
                    }
                    // append browsing event
                    _browsingEvents.Add(queryEvent);
                }
            }

            // store event locally in JSON
            string eventJson = LowercaseJsonSerializer.SerializeObject(queryEvent);
            _localLogger.WriteLine(eventJson);
        }

        public List<QueryEvent> GetAndClearBrowsingEvents()
        {
            lock (_browsingEventsLock)
            {
                List<QueryEvent> eventsToReturn = _browsingEvents;
                _browsingEvents = new List<QueryEvent>();
                return eventsToReturn;
            }
        }

        public void Dispose()
        {
            _localLogger.Dispose();
        }

        //public void ClearBrowsingEvents()
        //{
        //    lock (_browsingEventsLock)
        //    {
        //        _browsingEvents.Clear();
        //    }
        //}
    }
}
