using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.ActionLogging;

namespace ViretTool.BusinessLayer.Submission
{
    public class SubmissionService : ISubmissionService
    {
        private const string BaseUrl = "http://demo2.itec.aau.at:80/vbs/submit";
        private readonly HttpClient _client = new HttpClient();
        private readonly IInteractionLogger _interactionLogger;

        public SubmissionService(IInteractionLogger interactionLogger)
        {
            _interactionLogger = interactionLogger;
        }

        public async Task<string> SubmitFramesAsync(FrameToSubmit frameToSubmit)
        {
            string url = GetUrl(_interactionLogger.TeamId, _interactionLogger.MemberId, frameToSubmit);
            StringContent content = new StringContent(GetContent(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                _interactionLogger.ResetLog();
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SubmitLog()
        {
            string url = GetUrl(_interactionLogger.TeamId, _interactionLogger.MemberId);
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
            return $"{BaseUrl}?team={teamId}&member={memberId}&video={frameToSubmit.VideoId}&frame={frameToSubmit.FrameId}&shot={frameToSubmit.ShotId}";
        }

        private static string GetUrl(int teamId, int memberId)
        {
            return $"{BaseUrl}?team={teamId}&member={memberId}";
        }

        private string GetContent()
        {
            return LowercaseJsonSerializer.SerializeObject(_interactionLogger.Events);
        }
    }
}
