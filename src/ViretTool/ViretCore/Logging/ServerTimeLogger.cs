using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Viret.Logging.DresApi;

namespace Viret.Logging
{
    public class ServerTimeLogger : IDisposable
    {
        public string ServerUrl { get; set; }

        public string SessionId { get; set; }

        private readonly StreamWriter _localLogger;

        public ServerTimeLogger(string serverUrl, string sessionId)
        {
            ServerUrl = serverUrl;
            SessionId = sessionId;

            string logDirectory = Path.Combine("Logs", "ServerTime");
            Directory.CreateDirectory(logDirectory);
            string logFilename = $"ServerTimes_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _localLogger = new StreamWriter(Path.Combine(logDirectory, logFilename))
            {
                AutoFlush = true
            };
            _localLogger.WriteLine("Local time; Server time; Client difference");
        }


        public void LogServerTime(long localTimestamp)
        {
            try
            {
                string serverTimeJson = SendQueryToServer($"{ServerUrl}/api/status/time?session={SessionId}");
                CurrentTime serverTime = JsonConvert.DeserializeObject<CurrentTime>(serverTimeJson);
                _localLogger.WriteLine($"{localTimestamp};{serverTime.Timestamp};{localTimestamp - serverTime.Timestamp}");

            }
            catch (Exception ex)
            {
                _localLogger.WriteLine($"{localTimestamp};ERROR;ERROR: {ex.Message}");
            }
        }

        private string SendQueryToServer(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            using (HttpResponseMessage response = httpClient.GetAsync(url).Result)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public void Dispose()
        {
            _localLogger.Dispose();
        }
    }
}
