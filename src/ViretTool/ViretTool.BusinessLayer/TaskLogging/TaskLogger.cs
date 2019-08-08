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
        private const string LogDirectory = "TaskLogs";
        private const string ServerQuery = "/competition-state/get-active-competition-tasks";

        private readonly ILogger _logger;
        private readonly object _lockObject = new object();
        private string _queryUrl { get; set; }


        public TaskLogger(ILogger logger)
        {
            _logger = logger;
            string submissionUrl = ConfigurationManager.AppSettings["submissionUrl"];
            Uri uri = new Uri(submissionUrl);
            _queryUrl = uri.GetLeftPart(UriPartial.Authority) + ServerQuery;
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
                HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(3) };
                HttpResponseMessage responseMessage = await _httpClient.GetAsync(_queryUrl);
                string responseString = await responseMessage.Content.ReadAsStringAsync();
                return responseString;
            }
            catch (Exception ex)
            {
                string message = $"Error fetching task list: \"{_queryUrl}\"";
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
