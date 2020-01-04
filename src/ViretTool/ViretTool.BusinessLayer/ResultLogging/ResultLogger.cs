using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly string LogDirectory = Path.Combine("Logs", "ResultSampledLogs");
        private readonly ILogger _logger;
        private readonly IDatasetServicesManager _datasetServicesManager;



        public ResultLogger(ILogger logger, IDatasetServicesManager datasetServicesManager)
        {
            _logger = logger;
            _datasetServicesManager = datasetServicesManager;
        }


        public async Task LogResultSet(BiTemporalRankedResultSet resultSet, long unixTimestamp)
        {
            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(LogDirectory);
                    string filename = $"ResultSampledLog_{Environment.MachineName}_{unixTimestamp}.txt";
                    using (StreamWriter writer = new StreamWriter(Path.Combine(LogDirectory, filename)))
                    {
                        Dataset dataset = _datasetServicesManager.CurrentDataset.DatasetService.Dataset;
                        writer.WriteLine($"Videos: {dataset.Videos.Count}, Shots: {dataset.Shots.Count}, Frames: {dataset.Frames.Count}");

                        writer.WriteLine($"Former temporal result set: { string.Join("; ", SampleResultSet(resultSet.FormerTemporalResultSet))}");
                        writer.WriteLine($"Latter temporal result set: { string.Join("; ", SampleResultSet(resultSet.LatterTemporalResultSet))}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error while storing task logs to disk.", ex);
                    throw;
                }
            });
        }

        private List<string> SampleResultSet(List<PairedRankedFrame> resultSet)
        {
            List<string> result = new List<string>();
            for (int i = 1; i < resultSet.Count; i *= 2)
            {
                PairedRankedFrame frame = resultSet[i - 1];

                result.Add($"{(i - 1).ToString("00000000")}|{frame.Id.ToString("00000000")}|{frame.Rank.ToString("000000.0000000000;-00000.0000000000", CultureInfo.InvariantCulture)}|{frame.PairId.ToString("00000000")}");
            }
            return result;
        }

        public void Dispose()
        {
            
        }
    }
}
