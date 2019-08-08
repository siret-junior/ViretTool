using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.TaskLogging
{
    public interface ITaskLogger : IDisposable
    {
        string SubmissionUrl { get; set; }
        string ServerAddress { get; }
        string QueryPath { get; }
        string QueryUrl { get; }

        void FetchAndStoreTaskList();
    }
}
