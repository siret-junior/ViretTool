using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;

namespace ViretTool.BusinessLayer.Submission
{
    public class SubmissionService : ISubmissionService
    {
        private const string BaseUrl = "http://demo2.itec.aau.at:80/vbs/submit";
        private const string SUBMISSION_LOG_DIRECTORY = "SubmissionLogs";
        private readonly HttpClient _client = new HttpClient();
        private readonly IInteractionLogger _interactionLogger;
        private readonly ILogger _logger;

        public SubmissionService(IInteractionLogger interactionLogger, ILogger logger)
        {
            _interactionLogger = interactionLogger;
            _logger = logger;
        }

        public string SubmissionUrl { get; set; } = BaseUrl;

        public async Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit)
        {
            _interactionLogger.Log.Type = SubmissionType.Submit;
            string url = GetUrl(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId, frameToSubmit);
            string jsonInteractionLog = GetContent();
            StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(url, content);

            // log to disk
            StoreSubmissionLog(url, jsonInteractionLog);

            if (response.IsSuccessStatusCode)
            {
                _interactionLogger.ResetLog();
            }
            
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SubmitLog()
        {
            _interactionLogger.Log.Type = SubmissionType.Flush;
            string url = GetUrl(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId);
            string jsonInteractionLog = GetContent();
            StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(url, content);

            // log to disk
            StoreSubmissionLog(url, jsonInteractionLog);
            if (response.IsSuccessStatusCode)
            {
                _interactionLogger.ResetLog();
            }

            return await response.Content.ReadAsStringAsync();
        }

        private void StoreSubmissionLog(string url, string jsonInteractionLog)
        {
            try
            {
                Directory.CreateDirectory(SUBMISSION_LOG_DIRECTORY);
                string filename = 
                    "Submission_" + Environment.MachineName + "_"
                    + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss.ffff")
                    + ".txt";
                string filePath = Path.Combine(SUBMISSION_LOG_DIRECTORY, filename);
                File.WriteAllLines(filePath, new string[] { url, jsonInteractionLog });
            }
            catch (Exception ex)
            {
                _logger.Error("Error while storing logs to disk.", ex);
            }
        }

        private string GetUrl(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            return $"{SubmissionUrl}?team={teamId}&member={memberId}&video={frameToSubmit.VideoId}&frame={frameToSubmit.FrameNumber}";
        }

        private string GetUrl(int teamId, int memberId)
        {
            return $"{SubmissionUrl}?team={teamId}&member={memberId}";
        }

        private string GetContent()
        {
            _interactionLogger.Log.TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return LowercaseJsonSerializer.SerializeObject(_interactionLogger.Log);
        }
    }
}
