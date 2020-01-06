using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.Submission
{
    public interface IQueryPersistingService
    {
        long SaveQuery(BiTemporalQuery query);
        BiTemporalQuery LoadQuery(string path);
        void SaveTestObjects(int videoId, IList<int> frameNumbers);
        void SaveTestEnd();
    }

    public class QueryPersistingService : IQueryPersistingService
    {
        private readonly IInteractionLogger _interactionLogger;
        private readonly string QueryHistoryDirectory = Path.Combine("Logs", "QueryLogs");
        private readonly string TestDirectory = Path.Combine("Logs", "TestsLog");

        public QueryPersistingService(IInteractionLogger interactionLogger)
        {
            _interactionLogger = interactionLogger;
            if (!Directory.Exists(QueryHistoryDirectory))
            {
                Directory.CreateDirectory(QueryHistoryDirectory);
            }
            if (!Directory.Exists(TestDirectory))
            {
                Directory.CreateDirectory(TestDirectory);
            }
        }

        public long SaveQuery(BiTemporalQuery query)
        {
            // TODO: lock on Log object
            lock (this)
            {
                long lastInteractionTimeStamp = _interactionLogger.GetLastInteractionTimestamp();
                long currentQueryTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                Task.Run(() => 
                {
                    string jsonQuery = LowercaseJsonSerializer.SerializeObjectIndented(query);
                    File.WriteAllText(
                        Path.Combine(
                            QueryHistoryDirectory, 
                            $"QueryLog_{Environment.MachineName}_{lastInteractionTimeStamp}_{currentQueryTimeStamp}.json"), 
                        jsonQuery);
                });
                
                return currentQueryTimeStamp;
            }
        }

        public void SaveTestObjects(int videoId, IList<int> frameNumbers)
        {
            lock (this)
            {
                long actualTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string jsonTestData = LowercaseJsonSerializer.SerializeObjectIndented(new { VideoId = videoId, FrameNumbers = frameNumbers });

                File.WriteAllText(Path.Combine(TestDirectory, $"{actualTimeStamp}.json"), jsonTestData);
            }
        }

        public void SaveTestEnd()
        {
            lock (this)
            {
                long actualTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                File.Create(Path.Combine(TestDirectory, $"{actualTimeStamp}_end.json"));
            }
        }

        public BiTemporalQuery LoadQuery(string path)
        {
            return LowercaseJsonSerializer.DeserializeObject<BiTemporalQuery>(File.ReadAllText(path));
        }
    }
}
