using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.ResultLogging;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.Submission
{
    public abstract class SubmissionServiceBase : ISubmissionService
    {
        protected static readonly int MAX_RESULTS_COUNT = 10_000;

        private readonly HttpClient _client = new HttpClient()
        {
            Timeout = TimeSpan.FromMilliseconds(int.Parse(ConfigurationManager.AppSettings["networkTimeout"]))
        };
        private readonly IInteractionLogger _interactionLogger;
        protected readonly ILogger _logger;
        protected readonly IDatasetServicesManager _datasetServicesManager;
        private readonly string _networkLogDirectory = Path.Combine("Logs", "NetworkLogs");
        private readonly string _networkLogDirectoryPOST = Path.Combine("Logs", "NetworkLogsPOST");
        private readonly string _resultLogDirectory = Path.Combine("Logs", "ResultLogs");
        private readonly StreamWriter _streamWriterNetwork;
        private readonly StreamWriter _streamWriterNetworkPOST;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly int _sendLogsInterval = int.Parse(ConfigurationManager.AppSettings["sendLogsInterval"] ?? "30") * 1000;
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public string SubmissionUrl { get; set; }
        public string InteractionLoggingUrl { get; set; }
        public string ResultLoggingUrl { get; set; }
        
        public string SessionId { get; set; }


        public SubmissionServiceBase(IInteractionLogger interactionLogger, ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            // assign dependencies
            _interactionLogger = interactionLogger;
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            
            // load URLs from config file
            SubmissionUrl = ConfigurationManager.AppSettings["submissionUrl"];
            InteractionLoggingUrl = ConfigurationManager.AppSettings["interactionLoggingUrl"];
            ResultLoggingUrl = ConfigurationManager.AppSettings["resultLoggingUrl"];
            SessionId = ConfigurationManager.AppSettings["sessionId"];

            // create local logging directiories
            Directory.CreateDirectory(_networkLogDirectory);
            Directory.CreateDirectory(_networkLogDirectoryPOST);
            Directory.CreateDirectory(_resultLogDirectory);
            
            // setup network loggers
            string logFilename = $"NetworkLog_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _streamWriterNetwork = new StreamWriter(Path.Combine(_networkLogDirectory, logFilename))
            {
                AutoFlush = true
            };
            _streamWriterNetworkPOST = new StreamWriter(Path.Combine(_networkLogDirectoryPOST, logFilename))
            {
                AutoFlush = true
            };

            // setup periodic log submission
            _timer.Elapsed += async (sender, args) => await SubmitLogAsync();
            _timer.AutoReset = true;
            _timer.Interval = _sendLogsInterval;
            _timer.Start();
        }


        public async Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit)
        {
            try
            {
                if (!_datasetServicesManager.IsDatasetOpened)
                {
                    throw new InvalidOperationException("Dataset is not opened.");
                }

                _interactionLogger.Log.Type = SubmissionType.Submit;
                string url = GetUrlForSubmission(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId, frameToSubmit);

                
                // Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                string responseString;
                await _semaphore.WaitAsync();
                try
                {
                    HttpResponseMessage response = await GetAsyncLogged(url);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                finally
                {
                    // When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    // This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphore.Release();

                    // interaction logs are sent separately to a different logging endpoint
                    await SubmitLogAsync();
                }

                return responseString;
            }
            catch (Exception ex)
            {
                string message = "Error while submitting a searched frame.";
                _logger.Error(message, ex);
                return message;
            }
        }

        public async Task<string> SubmitLogAsync()
        {
            try
            {
                _interactionLogger.Log.Type = SubmissionType.Flush;
                string url = GetUrlForInteractionLogging(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId);
                string jsonInteractionLog = _interactionLogger.GetContent();
                StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
                
                // Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                await _semaphore.WaitAsync();
                try
                {
                    HttpResponseMessage response = await PostAsyncLogged(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        _interactionLogger.ResetLog();
                    }
                    return await response.Content.ReadAsStringAsync();
                }
                finally
                {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                string message = "Error while submitting interaction log.";
                _logger.Error(message, ex);
                return message;
            }
        }

        public async Task<string> SubmitResultsAsync(BiTemporalQuery query, BiTemporalRankedResultSet resultSet, long unixTimestamp)
        {
            try
            {
                string url = GetUrlForResultLogging(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId);
                
                IResultLog resultLog = GetResultLog(query, resultSet);
                StoreResultLog(resultLog.GetJsonIndented(unixTimestamp), unixTimestamp);
                string jsonResultLog = resultLog.GetJson(unixTimestamp);

                StringContent content = new StringContent(jsonResultLog, Encoding.UTF8, "application/json");
                
                //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                await _semaphore.WaitAsync();
                try
                {
                    HttpResponseMessage response = await PostAsyncLogged(url, content);
                    return await response.Content.ReadAsStringAsync();
                }
                finally
                {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                string message = "Error while submitting result logs.";
                _logger.Error(message, ex);
                return message;
            }
        }

        protected abstract IResultLog GetResultLog(BiTemporalQuery query, BiTemporalRankedResultSet biTemporalResultSet);

        private void StoreResultLog(string resultLog, long timestamp)
        {
            Task.Run(() =>
            {
                File.WriteAllText(Path.Combine(_resultLogDirectory, $"ResultLog_Top{MAX_RESULTS_COUNT}_{Environment.MachineName}_{timestamp}.json"), resultLog);
            });
        }

        protected abstract string GetUrlForSubmission(int teamId, int memberId, FrameToSubmit frameToSubmit);

        protected virtual string GetUrlForInteractionLogging(int teamId, int memberId)
        {
            return $"{InteractionLoggingUrl}?team={teamId}&member={memberId}&session={SessionId}";
        }

        protected virtual string GetUrlForResultLogging(int teamId, int memberId)
        {
            return $"{ResultLoggingUrl}?team={teamId}&member={memberId}&session={SessionId}";
        }


        private async Task<HttpResponseMessage> GetAsyncLogged(string url)
        {
            _streamWriterNetwork.WriteLine($"################################################################################");
            _streamWriterNetwork.WriteLine($"#### URL: {url}");
            _streamWriterNetwork.WriteLine($"--------------------------------------------------------------------------------");
            
            

            HttpResponseMessage response = await _client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();

            _streamWriterNetwork.WriteLine($"#### Response code: {(int)response.StatusCode} ({response.StatusCode})");
            _streamWriterNetwork.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriterNetwork.WriteLine($"#### Response content: {responseContent}");

            
            return response;
        }

        private async Task<HttpResponseMessage> PostAsyncLogged(string url, StringContent content)
        {
            string requestContent = await content.ReadAsStringAsync();
            _streamWriterNetwork.WriteLine($"################################################################################");
            _streamWriterNetwork.WriteLine($"#### URL: {url}");
            _streamWriterNetwork.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriterNetwork.WriteLine($"#### POST content ignored (length {requestContent.Length} characters).");
            _streamWriterNetwork.WriteLine($"--------------------------------------------------------------------------------");

            _streamWriterNetworkPOST.WriteLine($"################################################################################");
            _streamWriterNetworkPOST.WriteLine($"#### URL: {url}");
            _streamWriterNetworkPOST.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriterNetworkPOST.WriteLine($"#### POST content: {requestContent}");
            _streamWriterNetworkPOST.WriteLine($"--------------------------------------------------------------------------------");


            HttpResponseMessage response = await _client.PostAsync(url, content);
            string responseContent = await response.Content.ReadAsStringAsync();

            _streamWriterNetwork.WriteLine($"#### Response code: {(int)response.StatusCode} ({response.StatusCode})");
            _streamWriterNetwork.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriterNetwork.WriteLine($"#### Response content: {responseContent}");
            
            _streamWriterNetworkPOST.WriteLine($"#### Response code: {(int)response.StatusCode} ({response.StatusCode})");
            _streamWriterNetworkPOST.WriteLine($"--------------------------------------------------------------------------------");
            _streamWriterNetworkPOST.WriteLine($"#### Response content: {responseContent}");
            
            return response;
        }


    }
}
