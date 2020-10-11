using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ViretTool.BusinessLayer.Submission
{
    public class CamelcaseJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelcaseContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string SerializeObjectIndented(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);
        }

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, Settings);
        }

        public static T DeserializeObject<T>(string objectData)
        {
            return JsonConvert.DeserializeObject<T>(objectData, Settings);
        }

        private class CamelcaseContractResolver : DefaultContractResolver
        {
            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
            }
        }
    }
}
