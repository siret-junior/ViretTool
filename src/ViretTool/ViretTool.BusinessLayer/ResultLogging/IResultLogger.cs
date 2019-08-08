using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal;

namespace ViretTool.BusinessLayer.ResultLogging
{
    public interface IResultLogger : IDisposable
    {
        Task LogResultSet(BiTemporalRankedResultSet resultSet, long unixTimestamp);
    }
}
