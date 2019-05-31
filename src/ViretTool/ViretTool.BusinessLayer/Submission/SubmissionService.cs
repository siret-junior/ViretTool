using System;
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
        private const string BaseUrl = "http://demo2.itec.aau.at:80/vbs/submit";
        private readonly HttpClient _client = new HttpClient();
        private readonly IInteractionLogger _interactionLogger;
        private readonly ILogger _logger;
        private readonly IDatasetServicesManager _datasetServicesManager;

        public SubmissionService(IInteractionLogger interactionLogger, ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            _interactionLogger = interactionLogger;
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
        }

        public string SubmissionUrl { get; set; } = BaseUrl;

        public async Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit)
        {
            if (!_datasetServicesManager.IsDatasetOpened)
            {
                throw new InvalidOperationException("Dataset is not opened");
            }

            _interactionLogger.Log.Type = SubmissionType.Submit;
            string url = GetUrl(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId, frameToSubmit);

            string jsonInteractionLog = _interactionLogger.GetContent();
            HttpResponseMessage response;
            if (_datasetServicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
            {
                response = await _client.GetAsync(url);
            }
            else
            {
                StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
                response = await _client.PostAsync(url, content);
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
            HttpResponseMessage response = await _client.PostAsync(url, content);

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
    }
}
