//
// HttpContentExtensions.cs
//
// Author:
//       Sim Tsai <13759975+simhgd@users.noreply.github.com>
//
// Copyright (c) 2018 Alibaba Cloud
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

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
