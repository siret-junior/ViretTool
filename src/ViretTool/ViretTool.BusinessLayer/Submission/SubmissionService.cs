using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.Submission
{
    public class SubmissionService : ISubmissionService
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly IInteractionLogger _interactionLogger;
        private readonly ILogger _logger;
        private readonly IDatasetServicesManager _datasetServicesManager;
        private readonly string _submissionLogDirectory = "SubmissionLogs";
        private readonly StreamWriter _streamWriter;

        public SubmissionService(IInteractionLogger interactionLogger, ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            _interactionLogger = interactionLogger;
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            SubmissionUrl = ConfigurationManager.AppSettings["submissionUrl"];
            Directory.CreateDirectory(_submissionLogDirectory);
            string logFilename = $"SubmissionLog_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _streamWriter = new StreamWriter(Path.Combine(_submissionLogDirectory, logFilename))
            {
                AutoFlush = true
            };
        }

        public string SubmissionUrl { get; set; }

        public async Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit)
        {
            if (!_datasetServicesManager.IsDatasetOpened)
            {
                throw new InvalidOperationException("Dataset is not opened");
            }

            _interactionLogger.Log.Type = SubmissionType.Submit;
            string url = GetUrl(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId, frameToSubmit);

            HttpResponseMessage response;
            if (_datasetServicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
            {
                response = await _client.GetAsync(url);
            }
            else
            {
                string jsonInteractionLog = _interactionLogger.GetContent();
                StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
                //response = await _client.PostAsync(url, content);
                response = await PostAsyncLogged(url, content);
                if (response.IsSuccessStatusCode)
                {
                    _interactionLogger.ResetLog();
                }
            }
            
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SubmitLog()
        {
            _interactionLogger.Log.Type = SubmissionType.Flush;
            string url = GetUrl(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId);
            string jsonInteractionLog = _interactionLogger.GetContent();
            StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
            //HttpResponseMessage response = await _client.PostAsync(url, content);
            HttpResponseMessage response = await PostAsyncLogged(url, content);

            if (response.IsSuccessStatusCode)
            {
                _interactionLogger.ResetLog();
            }

            return await response.Content.ReadAsStringAsync();
        }

        private string GetUrl(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            if (!_datasetServicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
            {
                return $"{SubmissionUrl}?team={teamId}&member={memberId}&video={frameToSubmit.VideoId + 1}&frame={frameToSubmit.FrameNumber}";
            }

            int frameId = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(frameToSubmit.VideoId, frameToSubmit.FrameNumber);
            LifelogFrameMetadata metadata = _datasetServicesManager.CurrentDataset.LifelogDescriptorProvider[frameId];
            return $"{SubmissionUrl}?team={teamId}&image={metadata.FileName}.jpg";
        }

        private string GetUrl(int teamId, int memberId)
        {
            return $"{SubmissionUrl}?team={teamId}&member={memberId}";
        }


        private async Task<HttpResponseMessage> PostAsyncLogged(string url, StringContent content)
        {
            string requestContent = await content.ReadAsStringAsync();
            HttpResponseMessage response = await _client.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            _streamWriter.WriteLine($"#### URL: {url}");
            _streamWriter.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriter.WriteLine($"#### POST content: {requestContent}");
            _streamWriter.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriter.WriteLine($"#### Response code: {(int)response.StatusCode} ({response.StatusCode})");
            _streamWriter.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriter.WriteLine($"#### Response content: {responseContent}");
            _streamWriter.WriteLine($"################################################################################");

            return response;
        }
    }
}
