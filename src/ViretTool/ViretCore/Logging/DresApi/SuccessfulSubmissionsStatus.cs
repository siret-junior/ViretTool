using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Logging.DresApi
{
    public enum SubmissionOutcomes
    {
        CORRECT, WRONG, INDETERMINATE, UNDECIDABLE
    }
    public class SuccessfulSubmissionsStatus
    {
        public SubmissionOutcomes? Submission { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
    }
}
