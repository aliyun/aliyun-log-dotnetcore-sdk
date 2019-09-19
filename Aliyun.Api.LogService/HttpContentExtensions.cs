using System.Net.Http;
using System.Threading.Tasks;
#if NETSTANDARD2_0
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif

namespace Aliyun.Api.LogService
{
    public static class HttpContentExtensions
    {
        async public static Task<T> ReadAsAsync<T>(this HttpContent httpContent)
        {
            T result = default;
            var content = await httpContent.ReadAsStringAsync();
#if NETSTANDARD2_0
            result = JsonConvert.DeserializeObject<T>(content, JsonSettings.Default);
#else
            result = JsonSerializer.Deserialize<T>(content, JsonSettings.Default);
#endif
            return result;
        }
    }
}
