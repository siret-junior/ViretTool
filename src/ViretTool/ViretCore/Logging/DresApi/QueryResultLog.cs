using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viret.Logging.Json;

namespace Viret.Logging.DresApi
{
    /// <summary>
    /// Adapted from:
    /// https://github.com/dres-dev/DRES/blob/master/doc/oas-client.json
    /// </summary>
    public class QueryResultLog
    {
        public long TimeStamp;
        public string SortType;
        public string ResultSetAvailability;

        public List<QueryResult> Results { get; }
        public List<QueryEvent> Events { get; }


        public QueryResultLog(long timestamp, List<QueryResult> resultSet, QueryEvent query, string sortType, string resultSetAvailability)
        {
            TimeStamp = timestamp;
            SortType = sortType;
            ResultSetAvailability = resultSetAvailability;
            Results = resultSet;
            Events = new List<QueryEvent>();
            if (query != null)
            {
                Events.Add(query);
            }
        }

        public string ToJson(bool isIndented = false)
        { 
            return CamelcaseJsonSerializer.SerializeObject(this, isIndented);
        }
    }
}
