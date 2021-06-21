using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Viret.Logging.DresApi;

namespace Viret.Submission
{
    public class ItemSubmitter : IDisposable
    {
        public string SubmissionServerUrl { get; set; }

        public string SessionId { get; set; }

        private readonly StreamWriter _localLogger;

        public ItemSubmitter(string submissionServerUrl, string sessionId)
        {
            SubmissionServerUrl = submissionServerUrl;
            SessionId = sessionId;

            string logDirectory = Path.Combine("Logs", "Submissions");
            Directory.CreateDirectory(logDirectory);
            string logFilename = $"SubmissionLog_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _localLogger = new StreamWriter(Path.Combine(logDirectory, logFilename))
            {
                AutoFlush = true
            };
        }


        public string SubmitItem(int videoId, int frameId)
        {
            try
            {
                // V3C1 videoIds are decremented by 1
                string url = $"{SubmissionServerUrl}/submit?item={(videoId + 1):00000}&frame={frameId}&session={SessionId}";
                _localLogger.WriteLine("================================================================================\n"
                    + $"Sending submission at {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}:{_localLogger.NewLine}{url}");
                using (HttpClient httpClient = new HttpClient())
                using (HttpResponseMessage response = httpClient.GetAsync(url).Result)
                {
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    _localLogger.WriteLine("--------------------------------------------------------------------------------\n" 
                        + $"Response received at {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}:{_localLogger.NewLine}{responseString}");

                    SuccessfulSubmissionsStatus status = JsonConvert.DeserializeObject<SuccessfulSubmissionsStatus>(responseString);
                    //throw new Exception(status.Description);

                    return Enum.GetName(typeof(SubmissionOutcomes), status.Submission);
                }
            }
            catch (IOException ex)
            {
                throw new IOException($"Error storing submission log locally: {ex}", ex);
            }
            catch (HttpRequestException ex)
            {
                _localLogger.WriteLine("--------------------------------------------------------------------------------" 
                    + $"Error submitting candidate video {videoId} and frame {frameId}: {ex}");
                throw new HttpRequestException($"Error submitting candidate video {videoId} and frame {frameId}: {ex}", ex);
            }
        }

        public void Dispose()
        {
            _localLogger.Dispose();
        }
    }
}
