using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Submission
{
    public class SubmissionService : ISubmissionService
    {
        private const string _baseUrl = "http://demo2.itec.aau.at:80/vbs/submit";
        private static readonly HttpClient _client = new HttpClient();

        public async Task<string> SubmitFramesAsync(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            string url = GetUrl(teamId, memberId, frameToSubmit);
            FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]); //TODO logs
            HttpResponseMessage response = await _client.PostAsync(url, content);
            return await response.Content.ReadAsStringAsync();
        }

        private static string GetUrl(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            return $"{_baseUrl}?team={teamId}&member={memberId}&video={frameToSubmit.VideoId}&frame={frameToSubmit.FrameId}&shot={frameToSubmit.ShotId}";
        }
    }
}
