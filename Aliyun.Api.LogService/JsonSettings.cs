//
// JsonSettings.cs
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
