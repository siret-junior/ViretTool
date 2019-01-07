using System.IO;
using System.Linq;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;

namespace ViretTool.BusinessLayer.Submission
{
    public interface IQueryPersistingService
    {
        void SaveQuery(BiTemporalQuery query);
        BiTemporalQuery LoadQuery(string path);
    }

    public class QueryPersistingService : IQueryPersistingService
    {
        private readonly IInteractionLogger _interactionLogger;
        private const string QueryHistoryDirectory = "QueriesLog";

        public QueryPersistingService(IInteractionLogger interactionLogger)
        {
            _interactionLogger = interactionLogger;
            if (!Directory.Exists(QueryHistoryDirectory))
            {
                Directory.CreateDirectory(QueryHistoryDirectory);
            }
        }

        public void SaveQuery(BiTemporalQuery query)
        {
            long lastTimeStamp = _interactionLogger.Log.Events.Last(e => e.Category != LogCategory.Browsing).TimeStamp;
            string jsonQuery = LowercaseJsonSerializer.SerializeObject(query);

            File.WriteAllText(Path.Combine(QueryHistoryDirectory, $"{lastTimeStamp}.json"), jsonQuery);
        }

        public BiTemporalQuery LoadQuery(string path)
        {
            return LowercaseJsonSerializer.DeserializeObject<BiTemporalQuery>(File.ReadAllText(path));
        }
    }
}
