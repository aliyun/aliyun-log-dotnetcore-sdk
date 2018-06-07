//
// StsTokenAccessTest.cs
//
// Author:
//       MiNG <developer@ming.gz.cn>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Aliyun.Api.LogService.Tests
{
    public class StsTokenAccessTest
    {
        private static readonly String AccessKeyId = "<secret>";
        private static readonly String AccessKey = "<secret>";
        private static readonly String AssumerRoleArn = "acs:ram::<user_id>:role/<role_name>";

        private readonly ITestOutputHelper output;

        public StsTokenAccessTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestStsTokenAccess()
        {
            var query = new Dictionary<String, String>
            {
                {"Action", "AssumeRole"},
                {"RoleArn", AssumerRoleArn},
                {"RoleSessionName", "foo"},
                {"Format", "JSON"},
                {"Version", "2015-04-01"},
                {"AccessKeyId", AccessKeyId},
                {"SignatureMethod", "HMAC-SHA1"},
                {"SignatureVersion", "1.0"},
                {"SignatureNonce", Guid.NewGuid().ToString()},
                {"Timestamp", DateTimeOffset.UtcNow.ToString("yyyy-MM-dd\\Thh:mm:ss\\Z")},
            };

            var canonicalizedQueryString = String.Join("&", query
                .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}")
                .OrderBy(x => x));

            var signSource = $"GET&%2F&{Uri.EscapeDataString(canonicalizedQueryString)}";

            var hash = new HMACSHA1(Encoding.UTF8.GetBytes(AccessKey + "&")).ComputeHash(Encoding.UTF8.GetBytes(signSource));
            var signature = Convert.ToBase64String(hash);

            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://sts.aliyuncs.com"),
            };

            var response = await httpClient.GetAsync("/?" + $"{canonicalizedQueryString}&Signature={signature}");

            this.output.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}
