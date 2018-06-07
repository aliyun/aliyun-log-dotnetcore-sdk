//
// DefaultLogServiceClientBenchmark.cs
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
using System.Linq;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain.Log;
using BenchmarkDotNet.Attributes;

namespace Aliyun.Api.LogService.Benchmark
{
    public class DefaultLogServiceClientBenchmark
    {
        private const Int32 LogContentSize = 10;

        private ILogServiceClient client;

        private String logstoreName;

        private String cursor;

        private LogGroupInfo logGroup;

        [Params(5, 500)]
        public Int32 SizeK { get; set; }

        [GlobalSetup]
        public void Prepare()
        {
            this.client = LogServiceClientBuilders.HttpBuilder
                .Endpoint("cn-shenzhen.log.aliyuncs.com", "dotnet-sdk-sz")
                .Credential("<secret>", "<secret>")
                .Build();

            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            this.logstoreName = $"logstore-dotnet-sdk-test-{timestamp}";

            // Create logstore for benchmark
            this.client.CreateLogStoreAsync(this.logstoreName, 1, 1).Result.EnsureSuccess();
            Console.WriteLine($"// LogStore {this.logstoreName} created.");

            // Wait logstore prepared
            this.WaitLogStorePrepared().Wait();

            // Prepare data
            var logLines = this.SizeK * 1024 / (10 * LogContentSize) + 1;
            this.logGroup = new LogGroupInfo
            {
                Topic = "PressureTest",
                Source = "PressureTest",
                LogTags =
                {
                    {"Tag1", "Value1"},
                    {"Tag2", "Value2"}
                },
                Logs = Enumerable.Repeat(new LogInfo
                {
                    Time = DateTimeOffset.Now,
                    Contents = Enumerable.Range(0, LogContentSize)
                        .Select(i => (key: $"key{i}", value: $"value{i}"))
                        .ToDictionary(kv => kv.key, kv => kv.value)
                }, logLines).ToList()
            };
            Console.WriteLine($"// Data ready, assume size: {10 * LogContentSize * logLines / 1024}KB per request.");
        }

        private async Task WaitLogStorePrepared()
        {
            var successCount = 0;

            while (true)
            {
                var result = await this.client.GetCursorAsync(this.logstoreName, 0, "begin");
                if (result.IsSuccess)
                {
                    Console.WriteLine(
                        $"// {DateTime.Now} [{successCount}] - LogStore created verify ok, cursor was read, shardId=0, cursor={result.Result.Cursor}.");
                    // Require cotinued success 10 times.
                    if (++successCount >= 10)
                    {
                        Console.WriteLine($"// {DateTime.Now} - LogStore created verify pass, assumes LogStore has been prepared.");
                        this.cursor = result.Result.Cursor;
                        return;
                    }
                } else
                {
                    Console.WriteLine($"// {DateTime.Now} [{successCount}] - LogStore created verify failed, reset success count.");
                    successCount = 0;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        [GlobalCleanup]
        public void CleanUp()
        {
            var response = this.client.DeleteLogStoreAsync(this.logstoreName).Result;
            Console.WriteLine($"// CleanUp: {(response.IsSuccess ? "ok" : "failed")}");
        }

        [Benchmark(Baseline = true)]
        public async Task PostLogStoreLogs_Base()
        {
            await this.client.PostLogStoreLogsAsync(this.logstoreName, this.logGroup);
        }

        [BenchmarkCategory("PostLogStoreLogs")]
        [Benchmark(OperationsPerInvoke = 10)]
        public async Task PostLogStoreLogs_10Parallel()
        {
            await Task.WhenAll(Enumerable.Range(0, 10)
                .Select(async i =>
                    await this.client.PostLogStoreLogsAsync(this.logstoreName, this.logGroup)));
        }

        // [BenchmarkCategory("PostLogStoreLogs")]
        // [Benchmark(OperationsPerInvoke = 100)]
        // public async Task PostLogStoreLogs_100Parallel()
        // {
        //     await Task.WhenAll(Enumerable.Range(0, 100)
        //         .Select(async i =>
        //             await this.client.PostLogStoreLogsAsync(this.logstoreName, this.logGroup)));
        // }

        // [BenchmarkCategory("PullLogs")]
        // [Benchmark(Baseline = true)]
        // public async Task PullLogs_Base()
        // {
        //     await this.client.PullLogsAsync(this.logstoreName, 0, this.cursor, 10);
        // }
        //
        // [BenchmarkCategory("PullLogs")]
        // [Benchmark]
        // public async Task PullLogs_100Lines()
        // {
        //     await this.client.PullLogsAsync(this.logstoreName, 0, this.cursor, 100);
        // }
        //
        // [BenchmarkCategory("PullLogs")]
        // [Benchmark]
        // public async Task PullLogs_1000Lines()
        // {
        //     await this.client.PullLogsAsync(this.logstoreName, 0, this.cursor, 1000);
        // }
    }
}
