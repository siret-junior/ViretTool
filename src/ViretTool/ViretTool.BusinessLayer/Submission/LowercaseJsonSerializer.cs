using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ViretTool.BusinessLayer.Submission
{
    public class LowercaseJsonSerializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
                                                                  {
                                                                      ContractResolver = new LowercaseContractResolver(),
                                                                      NullValueHandling = NullValueHandling.Ignore
                                                                  };

        public static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);
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
