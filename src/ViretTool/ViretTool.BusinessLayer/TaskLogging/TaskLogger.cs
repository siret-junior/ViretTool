using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.TaskLogging
{
    public class TaskLogger : ITaskLogger
    {
        private const int HTTP_CLIENT_TIMEOUT_SECONDS = 3;
        private const string LogDirectory = "TaskLogs";
        
        private readonly ILogger _logger;
        private readonly object _lockObject = new object();

        public string QueryPath { get => "/competition-state/get-active-competition-tasks"; }
        public string ServerAddress { get; private set; }
        public string QueryUrl { get; private set; }

        private string _submissionUrl = "";
        public string SubmissionUrl
        {
            get => _submissionUrl;
            set
            {
                Uri uri = new Uri(value);
                ServerAddress = uri.GetLeftPart(UriPartial.Authority);
                QueryUrl = ServerAddress + QueryPath;
            }
        }
        

        public TaskLogger(ILogger logger)
        {
            _logger = logger;

            SubmissionUrl = ConfigurationManager.AppSettings["submissionUrl"];
        }

        // TODO: fix exception handling
        public async void FetchAndStoreTaskList()
        {
            try
            {
                string taskList = await FetchTaskList();
                StoreTaskList(taskList);
            }
            catch (Exception ex)
            {
                _logger.Error("Error fetching and storing task log.", ex);
            }
        }

        
        private async Task<string> FetchTaskList()
        {
            try
            {
                HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(HTTP_CLIENT_TIMEOUT_SECONDS) };
                HttpResponseMessage responseMessage = await _httpClient.GetAsync(QueryUrl);
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (Exception ex)
            {
                string message = $"Error fetching task list: \"{QueryUrl}\"";
                _logger.Error(message, ex);
                return message;
            }
        }


        private void StoreTaskList(string taskList)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                string filename = $"TaskLogs_{Environment.MachineName}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.txt";
                using (StreamWriter writer = new StreamWriter(Path.Combine(LogDirectory, filename)))
                {
                    writer.WriteLine(taskList);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error while storing task logs to disk.", ex);
                throw;
            }
        }
        

        public void Dispose()
        {
        }
    }
}
