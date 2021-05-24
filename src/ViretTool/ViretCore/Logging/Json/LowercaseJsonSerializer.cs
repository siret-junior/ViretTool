using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viret.Logging.Json
{
    public class LowercaseJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new LowercaseContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string SerializeObject(object obj, bool isIndented = false)
        {
            Formatting formatting = isIndented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(obj, formatting, Settings);
        }

        public static T DeserializeObject<T>(string objectData)
        {
            return JsonConvert.DeserializeObject<T>(objectData, Settings);
        }

        private class LowercaseContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.ToLower();
            }
        }
    }
}
