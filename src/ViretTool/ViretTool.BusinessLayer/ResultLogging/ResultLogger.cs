using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.Services;
using ViretTool.DataLayer.DataModel;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public class ResultLogger : IResultLogger
    {
        private const string LogDirectory = "ResultLogs";
        private readonly ILogger _logger;
        private readonly IDatasetServicesManager _datasetServicesManager;



        public ResultLogger(ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
        }


        public void LogResultSet(TemporalRankedResultSet resultSet, long unixTimestamp)
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
                string filename = $"ResultLog_{Environment.MachineName}_{unixTimestamp}.txt";
                using (StreamWriter writer = new StreamWriter(Path.Combine(LogDirectory, filename)))
                {
                    Dataset dataset = _datasetServicesManager.CurrentDataset.DatasetService.Dataset;
                    writer.WriteLine($"Dataset name: {dataset.DatasetName}");
                    writer.WriteLine($"Creation time: {dataset.DatasetCreationTime}");
                    writer.WriteLine($"Videos: {dataset.Videos.Count}, Shots: {dataset.Shots.Count}, Frames: {dataset.Frames.Count}");

                    writer.WriteLine(resultSet.TemporalQuery.ToString());

                    List<int> resultIdsSampledPrimary = SampleResultSet(resultSet.TemporalResultSets[0]);
                    List<int> resultIdsSampledSecondary = SampleResultSet(resultSet.TemporalResultSets[1]);

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
            throw new NotImplementedException();
        }
    }
}
