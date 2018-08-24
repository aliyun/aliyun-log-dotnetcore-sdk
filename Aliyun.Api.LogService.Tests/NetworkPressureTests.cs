//
// NetworkPressureTests.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Tests.TestUtils;
using Aliyun.Api.LogService.Utils;
using Xunit;

namespace Aliyun.Api.LogService.Tests
{
    [Trait("Skip", "true")]
    public class NetworkPressureTests : IClassFixture<TestContextFixture>, IDisposable
    {
        private const String LogPath = "/tmp/Aliyun.Api.LogService.Tests.NetworkPressureTests.log";
        private const Int32 Concurrency = 100;
        private const Int32 LogContentSize = 100;
        private const Int32 LogLines = 500;

        private readonly TestContextFixture context;
        private readonly TextWriter logWriter;

        public NetworkPressureTests(TestContextFixture context)
        {
            this.context = context;

            if (LogPath.IsNotEmpty())
            {
                this.logWriter = File.CreateText(LogPath);
            }

            this.Prepare();
        }

        private void Prepare()
        {
            this.context.Client.CreateLogStoreAsync(this.context.LogStoreName, 1, 1).Wait();
            this.Log($"LogStore [{this.context.LogStoreName}] created.");
            this.WaitLogStorePrepared().Wait();
        }

        public void Dispose()
        {
            this.context.Client.DeleteLogStoreAsync(this.context.LogStoreName).Wait();
            this.Log($"LogStore [{this.context.LogStoreName}] deleted.");
            this.logWriter.Dispose();
        }

        private void Log(String text)
        {
            text = $"[Thread-{Thread.CurrentThread.ManagedThreadId}] {DateTime.Now:O} {text}";
            // Debug.WriteLine(text);
            Trace.WriteLine(text);
            this.logWriter.WriteLine(text);
            this.logWriter.Flush();
        }

        private async Task WaitLogStorePrepared()
        {
            var successCount = 0;

            while (true)
            {
                var shardId = (await this.context.Client.ListShardsAsync(this.context.LogStoreName))
                    .EnsureSuccess()
                    .Result
                    .First()
                    .ShardId;

                var result = await this.context.Client.GetCursorAsync(this.context.LogStoreName, shardId, "begin");
                if (result.IsSuccess)
                {
                    this.Log($"[{successCount}] - LogStore created verify ok, cursor was read, shardId={shardId}, cursor={result.Result.Cursor}.");
                    // Require cotinued success 10 times.
                    if (++successCount >= 10)
                    {
                        this.Log("LogStore created verify pass, assumes LogStore has been prepared.");
                        return;
                    }
                } else
                {
                    this.Log($"[{successCount}] - LogStore created verify failed, reset success count.");
                    successCount = 0;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        [Fact]
        public async Task TestLongTimePressure()
        {
            var logGroup = new LogGroupInfo
            {
                Topic = "PressureTest",
                Source = "PressureTest",
                LogTags = new Dictionary<String, String>
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
                }, LogLines).ToList()
            };
            
            this.Log($"Test start, concurrency: {Concurrency}, estimate size of each request: {10 * LogContentSize * LogLines / 1000}K");

            async Task Run(Int32 group)
            {
                var totalExecution = 0;
                var lastExecution = 0;
                var lastErrorTime = DateTimeOffset.Now;
                var lastLogTime = DateTimeOffset.Now;
                while (true)
                {
                    try
                    {
                        var response = await this.context.Client.PostLogStoreLogsAsync(this.context.LogStoreName, logGroup);
                        response.EnsureSuccess();
                    } catch (Exception e)
                    {
                        var now = DateTimeOffset.Now;
                        var frequency = now - lastErrorTime;
                        lastErrorTime = now;
                        this.Log(
                            $"Group: {group}, Iteration: {totalExecution}, Frequency: {frequency.TotalMilliseconds}ms, Exception: {String.Join(" -> ", GetCacadeCause(e))}");
                    } finally
                    {
                        totalExecution++;
                        var inteval = DateTimeOffset.Now - lastLogTime;
                        if (inteval > TimeSpan.FromSeconds(10))
                        {
                            this.Log($"Group: {group}, Iteration: {totalExecution}, Executed: {totalExecution - lastExecution} in {inteval.TotalSeconds:0.00}s");
                            lastExecution = totalExecution;
                            lastLogTime = DateTimeOffset.Now;
                        }
                    }
                }
            }

            await Task.WhenAll(Enumerable.Range(0, Concurrency)
                .Select(i => Run(i))
                .ToArray());
        }

        private static IEnumerable<String> GetCacadeCause(Exception e)
        {
            var current = e;
            while (current != null)
            {
                yield return $"{current.GetType().Name}: {(current is LogServiceException lse ? $"{lse.ErrorCode} ({lse.ErrorMessage})" : current.Message)}";

                if (current.InnerException != null && current.InnerException != current)
                {
                    current = current.InnerException;
                } else
                {
                    break;
                }
            }
        }
    }
}
