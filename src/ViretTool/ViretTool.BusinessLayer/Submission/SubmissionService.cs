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
    public class SubmissionService : ISubmissionService
    {
        private static readonly int _maxResultsCount = 10_000;

        private readonly HttpClient _client = new HttpClient()
        {
            Timeout = TimeSpan.FromMilliseconds(int.Parse(ConfigurationManager.AppSettings["networkTimeout"]))
        };
        private readonly IInteractionLogger _interactionLogger;
        private readonly ILogger _logger;
        private readonly IDatasetServicesManager _datasetServicesManager;
        private readonly string _networkLogDirectory = Path.Combine("Logs", "NetworkLogs");
        private readonly string _networkLogDirectoryPOST = Path.Combine("Logs", "NetworkLogsPOST");
        private readonly string _resultLogDirectory = Path.Combine("Logs", "ResultLogs");
        private readonly StreamWriter _streamWriterNetwork;
        private readonly StreamWriter _streamWriterNetworkPOST;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly int _sendLogsInterval = int.Parse(ConfigurationManager.AppSettings["sendLogsInterval"] ?? "30") * 1000;
        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public SubmissionService(IInteractionLogger interactionLogger, ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            _interactionLogger = interactionLogger;
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
            
            SubmissionUrl = ConfigurationManager.AppSettings["submissionUrl"];
            LoggingUrl = ConfigurationManager.AppSettings["loggingUrl"];
            
            Directory.CreateDirectory(_networkLogDirectory);
            Directory.CreateDirectory(_networkLogDirectoryPOST);
            Directory.CreateDirectory(_resultLogDirectory);
            
            string logFilename = $"NetworkLog_{Environment.MachineName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss.ffff}.txt";
            _streamWriterNetwork = new StreamWriter(Path.Combine(_networkLogDirectory, logFilename))
            {
                AutoFlush = true
            };
            _streamWriterNetworkPOST = new StreamWriter(Path.Combine(_networkLogDirectoryPOST, logFilename))
            {
                AutoFlush = true
            };

            _timer.Elapsed += async (sender, args) => await SubmitLogAsync();
            _timer.AutoReset = true;
            _timer.Interval = _sendLogsInterval;
            _timer.Start();
        }

        public string SubmissionUrl { get; set; }
        public string LoggingUrl { get; set; }

        public async Task<string> SubmitFrameAsync(FrameToSubmit frameToSubmit)
        {
            try
            {
                if (!_datasetServicesManager.IsDatasetOpened)
                {
                    throw new InvalidOperationException("Dataset is not opened");
                }

                _interactionLogger.Log.Type = SubmissionType.Submit;
                string url = GetUrlForSubmission(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId, frameToSubmit);

                //HttpResponseMessage response;
                //if (_datasetServicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
                //{
                //    response = await _client.GetAsync(url);
                //}
                //else
                //{
                //    string jsonInteractionLog = _interactionLogger.GetContent();
                //    StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
                //    //response = await _client.PostAsync(url, content);
                //    response = await PostAsyncLogged(url, content);
                //    if (response.IsSuccessStatusCode)
                //    {
                //        _interactionLogger.ResetLog();
                //    }
                //}

                //HttpResponseMessage response = await _client.GetAsync(url);
                string responseString;
                if (_datasetServicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
                {
                    HttpResponseMessage response = await _client.GetAsync(url);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // submission is sent separately to a submission endpoint without interaction logs
                    StringContent content = new StringContent("{}", Encoding.UTF8, "application/json");

                    //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                    await _semaphoreSlim.WaitAsync();
                    try
                    {
                        HttpResponseMessage response = await PostAsyncLogged(url, content);
                        responseString = await response.Content.ReadAsStringAsync();
                    }
                    finally
                    {
                        //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                        //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                        _semaphoreSlim.Release();
                    }

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
                string url = GetUrlForLogging(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId);
                string jsonInteractionLog = _interactionLogger.GetContent();
                StringContent content = new StringContent(jsonInteractionLog, Encoding.UTF8, "application/json");
                //HttpResponseMessage response = await _client.PostAsync(url, content);

                //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                await _semaphoreSlim.WaitAsync();
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
                    _semaphoreSlim.Release();
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
                Result[] results = ConvertResults(resultSet);
                ResultLog resultLog = new ResultLog(query, results);
                string url = GetUrlForLogging(_interactionLogger.Log.TeamId, _interactionLogger.Log.MemberId);
                StoreResultLog(resultLog.GetContentIndented(unixTimestamp), unixTimestamp);
                string jsonResultLog = resultLog.GetContent(unixTimestamp);
                StringContent content = new StringContent(jsonResultLog, Encoding.UTF8, "application/json");
                
                //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
                await _semaphoreSlim.WaitAsync();
                try
                {
                    HttpResponseMessage response = await PostAsyncLogged(url, content);
                    return await response.Content.ReadAsStringAsync();
                }
                finally
                {
                    //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                    //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                    _semaphoreSlim.Release();
                }
            }
            catch (Exception ex)
            {
                string message = "Error while submitting result logs.";
                _logger.Error(message, ex);
                return message;
            }
        }

        private void StoreResultLog(string resultLog, long timestamp)
        {
            Task.Run(() =>
            {
                File.WriteAllText(Path.Combine(_resultLogDirectory, $"ResultLog_Top{_maxResultsCount}_{Environment.MachineName}_{timestamp}.json"), resultLog);
            });
        }

        private Result[] ConvertResults(BiTemporalRankedResultSet biTemporalResultSet)
        {
            List<PairedRankedFrame> resultSet;
            switch (biTemporalResultSet.TemporalQuery.PrimaryTemporalQuery)
            {
                case BiTemporalQuery.TemporalQueries.Former:
                    resultSet = biTemporalResultSet.FormerTemporalResultSet;
                    break;
                case BiTemporalQuery.TemporalQueries.Latter:
                    resultSet = biTemporalResultSet.LatterTemporalResultSet;
                    break;
                default:
                    throw new NotImplementedException("Unknown primary temporal query.");
            }


            return resultSet.Take(_maxResultsCount)
                .Select((item, index) => new Result(
                    _datasetServicesManager.CurrentDataset.DatasetService.GetVideoIdForFrameId(item.Id) + 1,
                    _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumberForFrameId(item.Id),
                    item.Rank,
                    index
                    ))
                .ToArray();
        }

        private string GetUrlForSubmission(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            if (!_datasetServicesManager.CurrentDataset.DatasetParameters.IsLifelogData)
            {
                return $"{SubmissionUrl}?team={teamId}&member={memberId}&video={frameToSubmit.VideoId + 1}&frame={frameToSubmit.FrameNumber}";
            }

            int frameId = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(frameToSubmit.VideoId, frameToSubmit.FrameNumber);
            LifelogFrameMetadata metadata = _datasetServicesManager.CurrentDataset.LifelogDescriptorProvider[frameId];
            return $"{SubmissionUrl}?team={teamId}&image={metadata.FileName}.jpg";
        }

        private string GetUrlForLogging(int teamId, int memberId)
        {
            return $"{LoggingUrl}?team={teamId}&member={memberId}";
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
