//
// TestContextFixture.cs
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

namespace Aliyun.Api.LogService.Tests.TestUtils
{
    public class TestContextFixture
    {
        public ILogServiceClient Client { get; }

        public Boolean ShouldInitProject { get; set; }
        public Boolean ShouldCleanProject { get; set; }
        public Boolean ShouldInit { get; set; }
        public Boolean ShouldClean { get; set; }
        public Boolean ShouldTestShipper { get; set; }

        public String ProjectName { get; set; }
        public String LogStoreName { get; set; }
        public String MachineGroupName { get; set; }
        public String ConfigName { get; set; }
        public String ShipperName { get; set; }

        public Int32 LogStoreTtl { get; set; } = 1;

        public String Cursor { get; set; }

        public Int32 ShardCount { get; set; } = 1;
        public Int32 WriteShardId { get; set; }
        public Int32 WholeShardId { get; set; }

        public String ShipperTaskId { get; set; }

        public TestContextFixture()
        {
            this.ShouldInitProject = false;
            this.ShouldCleanProject = false;
            this.ShouldInit = true;
            this.ShouldClean = true;
            this.ShouldTestShipper = false;

            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.ProjectName = "dotnet-sdk";
            this.LogStoreName = $"logstore-dotnet-sdk-test-{timestamp}";
            this.MachineGroupName = $"machinegroup-dotnet-sdk-test-{timestamp}";
            this.ConfigName = $"config-dotnet-sdk-test-{timestamp}";
            this.ShipperName = "shipper-dotnet-sdk-test";

            var (accessKey, accessSecret) = LoadCredential();
            this.Client = LogServiceClientBuilders.HttpBuilder
                .Endpoint("https://cn-qingdao.log.aliyuncs.com", this.ProjectName)
                .Credential(accessKey, accessSecret)
                .Build();
        }

        private static (String, String) LoadCredential()
            => Boolean.TryParse(Environment.GetEnvironmentVariable("CI"), out var isCi) && isCi
                ? (Environment.GetEnvironmentVariable("access_key"), Environment.GetEnvironmentVariable("access_secret"))
                : ("<secret>", "<secret>");
    }
}
