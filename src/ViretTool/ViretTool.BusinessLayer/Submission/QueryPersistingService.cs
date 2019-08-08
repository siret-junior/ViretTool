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
        private const string QueryHistoryDirectory = "QueriesLog";
        private const string TestDirectory = "TestsLog";

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
            lock (this)
            {
                long lastTimeStamp = _interactionLogger.Log.Events.LastOrDefault(e => e.Category != LogCategory.Browsing)?.TimeStamp ?? 0;
                long actualTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                Task.Run(() => 
                {
                    string jsonQuery = LowercaseJsonSerializer.SerializeObject(query);
                    File.WriteAllText(Path.Combine(QueryHistoryDirectory, $"{lastTimeStamp}_{actualTimeStamp}.json"), jsonQuery);
                });
                
                return actualTimeStamp;
            }
        }

        public void SaveTestObjects(int videoId, IList<int> frameNumbers)
        {
            lock (this)
            {
                long actualTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                string jsonTestData = LowercaseJsonSerializer.SerializeObject(new { VideoId = videoId, FrameNumbers = frameNumbers });

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
