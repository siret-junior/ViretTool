using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret
{
    public class Config
    {
        public string DresServer { get; set; }
        public string SessionId { get; set; }

        public static readonly Config Default = new Config
        {
            DresServer = "https://localhost",
            SessionId = ""
        };
    }
}
