using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Viret.Logging;
using Viret.Logging.DresApi;
using Viret.Logging.Json;
using Viret.Ranking.ContextAware;

namespace Viret.Submission
{
    /// <summary>
    /// Submits result log when result changes.
    /// Includes current query state (provided by InteractionLogger).
    /// Appends/flushes accumumated browsing events since previous submission (provided by InteractionLogger).
    /// </summary>
    public class LogSubmitter : IDisposable
    {
        public string LoggingServerUrl { get; set; }

        public string SessionId { get; set; }

        private readonly StreamWriter _localLogger;
        private readonly InteractionLogger _interactionLogger;

        public LogSubmitter(string loggingServerUrl, string sessionId, InteractionLogger interactionLogger)
        {
            LoggingServerUrl = loggingServerUrl;
            SessionId = sessionId;
            _interactionLogger = interactionLogger;

            string logDirectory = Path.Combine("Logs", "Results");
            Directory.CreateDirectory(logDirectory);
            string logFilename = $"ResultLog_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _localLogger = new StreamWriter(Path.Combine(logDirectory, logFilename))
            {
                AutoFlush = true
            };
        }


        /// <summary>
        /// Builds a result log from result set, current query state and browsing events that occured since last log submission.
        /// </summary>
        /// <param name="resultSet"></param>
        /// <param name="queryState"></param>
        /// <param name="browsingEvents"></param>
        public string SubmitResultLog(List<QueryResult> resultSet, QueryEvent queryState)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            QueryResultLog resultLog = new QueryResultLog(timestamp, resultSet, queryState, _interactionLogger.GetAndClearBrowsingEvents());
            StringContent httpPostContent = new StringContent(resultLog.ToJson(), Encoding.UTF8, "application/json");
            try
            {
                string url = $"{LoggingServerUrl}?&session={SessionId}";
                _localLogger.WriteLine("================================================================================");
                _localLogger.WriteLine($"Sending result log at {timestamp} to '{url}':{_localLogger.NewLine}{httpPostContent}");
                using (HttpClient httpClient = new HttpClient())
                using (HttpResponseMessage response = httpClient.PostAsync(url, httpPostContent).Result)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    _localLogger.WriteLine("--------------------------------------------------------------------------------");
                    _localLogger.WriteLine($"Response received at {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}:{_localLogger.NewLine}{responseString}");
                    return responseString;
                }
            }
            catch (IOException ex)
            {
                throw new IOException($"Error storing result set log locally: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _localLogger.WriteLine("--------------------------------------------------------------------------------");
                _localLogger.WriteLine($"Error at {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}:{_localLogger.NewLine}{ex}");
                // TODO: just log to a file, do not pass the exception
                throw new HttpRequestException($"Error submitting result set log: {ex.Message}", ex);
            }
            // TODO: do not clear browsing events when an error occurs
        }


        public void FlushBrowsingEvents()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _localLogger.Dispose();
        }
    }
}
