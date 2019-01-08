using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.ActionLogging;

namespace ViretTool.BusinessLayer.Submission
{
    public class SubmissionService : ISubmissionService
    {
        private const string BaseUrl = "http://demo2.itec.aau.at:80/vbs/submit";
        private const string SUBMISSION_LOG_DIRECTORY = "SubmissionLogs";
        private readonly HttpClient _client = new HttpClient();
        private readonly IInteractionLogger _interactionLogger;

        public SubmissionService(IInteractionLogger interactionLogger)
        {
            _interactionLogger = interactionLogger;
        }

        public async Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit)
        {
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

        private void StoreSubmissionLog(string url, string jsonInteractionLog)
        {
            try
            {
                Directory.CreateDirectory(SUBMISSION_LOG_DIRECTORY);
                string filename = 
                    "Submission_" 
                    + Environment.MachineName 
                    + DateTime.UtcNow.ToString("s");
                File.WriteAllLines(filename, new string[] { url, jsonInteractionLog });
            }
            catch (Exception ex)
            {
                //
            }
        }

        public async Task<string> SubmitLog()
        {
            string url = GetUrl(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId);
            StringContent content = new StringContent(GetContent(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                _interactionLogger.ResetLog();
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static string GetUrl(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            return $"{BaseUrl}?team={teamId}&member={memberId}&video={frameToSubmit.VideoId}&frame={frameToSubmit.FrameNumber}";
        }

        private static string GetUrl(int teamId, int memberId)
        {
            return $"{BaseUrl}?team={teamId}&member={memberId}";
        }

        private string GetContent()
        {
            return LowercaseJsonSerializer.SerializeObject(_interactionLogger.Log);
        }
    }
}
