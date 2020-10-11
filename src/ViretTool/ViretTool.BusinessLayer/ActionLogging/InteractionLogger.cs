using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Submission;

namespace ViretTool.BusinessLayer.ActionLogging
{
    public class InteractionLogger : IInteractionLogger
    {
        // TODO: elaborate what is this delay used for
        private const int TimeDelayMiliseconds = 1000;
        private readonly string LogDirectory = Path.Combine("Logs", "InteractionLogs");

        private readonly object _lockObject = new object();
        private readonly ILogger _logger;
        private readonly StreamWriter _streamWriter;

        public InteractionLogger(ILogger logger)
        {
            _logger = logger;

            Directory.CreateDirectory(LogDirectory);
            string filename = $"InteractionLogs_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _streamWriter = new StreamWriter(Path.Combine(LogDirectory, filename))
                            {
                                AutoFlush = true
                            };
        }

        public InteractionLog Log { get; } = new InteractionLog();

        public async void LogInteraction(LogCategory category, LogType type, string value = null/*, string attributes = null*/)
        {
            await Task.Run(
                () =>
                {
                    lock (_lockObject)
                    {
                        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        if (Log.Events.Count > 0 && Log.Events.Last().Category == category && Log.Events.Last().Type[0] == type)
                        {
                            long lastEventTime = Log.Events[Log.Events.Count - 1].Timestamp;
                            if (Math.Abs(lastEventTime - currentTime) < TimeDelayMiliseconds)
                            {
                                return;
                            }
                        }

                        Action action = new Action(currentTime, category, type, value/*, attributes*/);
                        Log.Events.Add(action);

                        StoreLog(LowercaseJsonSerializer.SerializeObjectIndented(action));
                    }
                });
        }

        public void ResetLog()
        {
            lock (_lockObject)
            {
                Log.Events.Clear();
            }
        }

        public string GetContent()
        {
            lock (_lockObject)
            {
                Log.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return CamelcaseJsonSerializer.SerializeObject(Log);
            }
        }

        private void StoreLog(string jsonInteractionLog)
        {
            try
            {
                _streamWriter.WriteLine(jsonInteractionLog);
            }
            catch (Exception ex)
            {
                _logger.Error("Error while storing logs to disk.", ex);
            }
        }

        public long GetLastInteractionTimestamp() 
        {
            lock (_lockObject)
            {
                return Log.Events.LastOrDefault(e => e.Category != LogCategory.Browsing)?.Timestamp ?? 0;
            }
        }

        public void Dispose()
        {
            _streamWriter?.Close();
            _streamWriter?.Dispose();
        }
    }
}
