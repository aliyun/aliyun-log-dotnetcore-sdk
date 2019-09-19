#if NETSTANDARD2_0
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#else
using System.Text.Json;
using System.Text.Json.Serialization;
#endif

namespace Aliyun.Api.LogService
{
    public static class JsonSettings
    {
#if NETSTANDARD2_0
        public static readonly JsonSerializerSettings Default;
#else
        public static readonly JsonSerializerOptions Default;
#endif

        static JsonSettings()
        {
#if NETSTANDARD2_0
            Default = new JsonSerializerSettings();
            Default.Formatting = Formatting.Indented;
            Default.Converters.Add(new StringEnumConverter());
            Default.ContractResolver = new CamelCasePropertyNamesContractResolver();
            Default.NullValueHandling = NullValueHandling.Ignore;
#else
            Default = new JsonSerializerOptions();
            Default.WriteIndented = true;
            Default.Converters.Add(new JsonStringEnumConverter());
            Default.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            Default.PropertyNameCaseInsensitive = true;
            Default.IgnoreNullValues = true;
#endif
        }
    }
}
