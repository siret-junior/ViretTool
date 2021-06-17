using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viret.Logging.DresApi;
using Viret.Ranking.ContextAware;

namespace Viret.Logging
{
    public class ResultLogger : IDisposable
    {
        public string LoggingServerUrl { get; set; }

        public string SessionId { get; set; }

        private readonly StreamWriter _localLogger;

        public ResultLogger(string loggingServerUrl, string sessionId)
        {
            LoggingServerUrl = loggingServerUrl;
            SessionId = sessionId;

            string logDirectory = Path.Combine("Logs", "Results");
            Directory.CreateDirectory(logDirectory);
            string logFilename = $"ResultLog_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _localLogger = new StreamWriter(Path.Combine(logDirectory, logFilename))
            {
                AutoFlush = true
            };
        }

        
        public QueryResultLog LogResultSet(List<QueryResult> resultSet, QueryEvent query, string sortType, string resultSetAvailability)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            QueryResultLog resultSetLog = new QueryResultLog(timestamp, resultSet, query, sortType, resultSetAvailability);
            _localLogger.WriteLine(resultSetLog.ToJson());
            return resultSetLog;
        }

        public void Dispose()
        {
            _localLogger.Dispose();
        }
    }
}
