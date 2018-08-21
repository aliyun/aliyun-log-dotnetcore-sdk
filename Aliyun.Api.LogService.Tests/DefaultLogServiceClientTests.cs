//
// DefaultLogServiceClientTests.cs
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain.Config;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Domain.LogStore.Index;
using Aliyun.Api.LogService.Domain.LogStore.Shard;
using Aliyun.Api.LogService.Domain.LogStore.Shipper;
using Aliyun.Api.LogService.Infrastructure.Protocol;
using Aliyun.Api.LogService.Infrastructure.Protocol.Http;
using Aliyun.Api.LogService.Tests.TestUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Aliyun.Api.LogService.Tests
{
    [TestCaseOrderer("Aliyun.Api.LogService.Tests.TestUtils.TestPriorityOrderer", "Aliyun.Api.LogService.Tests")]
    public class DefaultLogServiceClientTests : IClassFixture<TestContextFixture>
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings();

        static DefaultLogServiceClientTests()
        {
            Settings.Formatting = Formatting.Indented;
            Settings.Converters.Add(new StringEnumConverter());
        }

        private readonly ITestOutputHelper output;
        private readonly TestContextFixture context;

        public DefaultLogServiceClientTests(ITestOutputHelper output, TestContextFixture context)
        {
            this.output = output;
            this.context = context;
        }

        private void PrintResponse(IResponse response, Boolean printOriginContent = false)
        {
            var serializer = JsonSerializer.CreateDefault(Settings);

            var printObject = new JObject
            {
                {"IsSuccess", response.IsSuccess},
                {"StatusCode", new JValue(response.GetHttpStatusCode())},
                {"Headers", JToken.FromObject(response.Headers, serializer)},
            };

            void PrintOriginContent()
            {
                var formatter = new JsonMediaTypeFormatter();
                var originContent = response.GetHttpResponseMessage().Content;
                if (originContent != null && formatter.SupportedMediaTypes.Contains(originContent.Headers.ContentType))
                {
                    var originResult = originContent.ReadAsAsync<JToken>().Result;
                    printObject.Add("Data", originResult);
                }
            }

            if (response.IsSuccess)
            {
                var responseType = response.GetType();
                if (responseType.FindInterfaces((type, _) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IResponse<>), null).Any())
                {
                    Object result = ((dynamic) response).Result;
                    printObject.Add("Result", JToken.FromObject(result, serializer));
                } else
                {
                    printObject.Add("Result", JValue.CreateUndefined());
                }

                if (printOriginContent)
                {
                    PrintOriginContent();
                }
            } else
            {
                if (response.Error != null)
                {
                    printObject.Add("Error", JToken.FromObject(response.Error, serializer));
                } else
                {
                    printObject.Add("Error", JValue.CreateNull());
                }

                PrintOriginContent();
            }


            var serializeObject = JsonConvert.SerializeObject(printObject, Settings);
            this.output.WriteLine(serializeObject);
        }

        [Fact, TestPriority(nameof(TestCreateLogStore))]
        public async Task WaitLogStorePrepared()
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
                    this.output.WriteLine(
                        $"{DateTime.Now} [{successCount}] - LogStore created verify ok, cursor was read, shardId={shardId}, cursor={result.Result.Cursor}.");
                    // Require cotinued success 10 times.
                    if (++successCount >= 10)
                    {
                        return;
                    }
                } else
                {
                    successCount = 0;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        #region LogStore

        [Fact, TestPriority(Int32.MinValue, nameof(TestCreateProject))]
        public async Task TestCreateLogStore()
        {
            if (!this.context.ShouldInit)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.CreateLogStoreAsync(this.context.LogStoreName, this.context.LogStoreTtl, 1);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(nameof(TestGetLogStore))]
        public async Task TestUpdateLogStore()
        {
            var updateTtl = this.context.LogStoreTtl % 2 == 0 ? 1 : 2;
            var response = await this.context.Client.UpdateLogStoreAsync(this.context.LogStoreName, updateTtl, 1);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestGetLogStore()
        {
            var response = await this.context.Client.GetLogStoreAsync(this.context.LogStoreName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
            Assert.Equal(this.context.LogStoreName, response.Result.LogstoreName);

            this.context.LogStoreTtl = response.Result.Ttl;
            this.context.ShardCount = response.Result.ShardCount;
        }

        [Fact]
        public async Task TestListLogStore()
        {
            var response = await this.context.Client.ListLogStoreAsync(offset: 0, size: 100);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
            Assert.True(response.Result.Logstores.Contains(this.context.LogStoreName));
        }

        [Fact, TestPriority(Int32.MaxValue, nameof(TestDeleteConfig))]
        public async Task TestDeleteLogStore()
        {
            if (!this.context.ShouldClean)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.DeleteLogStoreAsync(this.context.LogStoreName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        #region Shard

        [Fact, TestPriority(nameof(TestUpdateLogStore))]
        public async Task TestListShards()
        {
            var response = await this.context.Client.ListShardsAsync(this.context.LogStoreName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.context.ShardCount = response.Result.Count;
            this.context.WriteShardId = response.Result
                .First(x => x.Status == ShardState.ReadWrite)
                .ShardId;
            this.context.WholeShardId = response.Result
                .First(x => x.InclusiveBeginKey == "00000000000000000000000000000000"
                            && x.ExclusiveEndKey == "ffffffffffffffffffffffffffffffff")
                .ShardId;
        }

        [Fact, TestPriority(nameof(TestListShards))]
        public async Task TestSplitShard()
        {
            var response = await this.context.Client.SplitShardAsync(this.context.LogStoreName, this.context.WriteShardId, "ef000000000000000000000000000000");
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.context.WriteShardId = response.Result
                .First(x => x.Status == ShardState.ReadWrite)
                .ShardId;
        }

        [Fact, TestPriority(nameof(TestSplitShard))]
        public async Task TestMergeShards()
        {
            var response = await this.context.Client.MergeShardsAsync(this.context.LogStoreName, this.context.WriteShardId);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.context.WriteShardId = response.Result
                .First(x => x.Status == ShardState.ReadWrite)
                .ShardId;
        }

        [Fact, TestPriority(nameof(WaitLogStorePrepared))]
        public async Task TestGetCursor()
        {
            var response = await this.context.Client.GetCursorAsync(this.context.LogStoreName, this.context.WholeShardId, "begin");
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.context.Cursor = response.Result.Cursor;
        }

        #endregion Shard

        #region Shipper

        [Fact, TestPriority(nameof(TestPostLogStoreLogs))]
        public async Task TestGetShipperStatus()
        {
            if (!this.context.ShouldTestShipper)
            {
                this.output.WriteLine("Skipped!");
                return;
            }

            var begin = DateTimeOffset.UtcNow;

            IResponse<GetShipperResult> response;
            var i = 1;
            do
            {
                response = await this.context.Client.GetShipperStatusAsync(
                    this.context.LogStoreName,
                    this.context.ShipperName,
                    begin.AddSeconds(-300),
                    begin.AddSeconds(300),
                    offset: 0,
                    size: 100);

                if (response.EnsureSuccess().Result.Count > 0)
                {
                    break;
                }

                this.output.WriteLine($"[{DateTimeOffset.UtcNow}] Try {i} times, retry after 10 seconds");
                await Task.Delay(TimeSpan.FromSeconds(10));
                i++;
            } while (DateTimeOffset.UtcNow - begin < TimeSpan.FromSeconds(330));

            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.context.ShipperTaskId = response.Result.Tasks.First().Id;
        }

        [Fact, TestPriority(nameof(TestGetShipperStatus))]
        public async Task TestRetryShipperTask()
        {
            if (!this.context.ShouldTestShipper)
            {
                this.output.WriteLine("Skipped!");
                return;
            }

            var response = await this.context.Client.RetryShipperTaskAsync(
                this.context.LogStoreName,
                this.context.ShipperName,
                this.context.ShipperTaskId);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        #endregion Shipper

        #region Index

        [Fact, TestPriority(Int32.MinValue, nameof(TestCreateLogStore))]
        public async Task TestCreateIndex()
        {
            if (!this.context.ShouldInit)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.CreateIndexAsync(
                this.context.LogStoreName,
                new IndexLineInfo(new[] {' ', ','}));
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        #endregion Index

        #endregion LogStore

        #region Log

        [Fact, TestPriority(nameof(WaitLogStorePrepared))]
        public async Task TestPostLogStoreLogs()
        {
            var response = await this.context.Client.PostLogStoreLogsAsync(this.context.LogStoreName, new LogGroupInfo
            {
                Topic = "UnitTest",
                Source = "UnitTest",
                LogTags = new Dictionary<String, String>
                {
                    {"Tag1", "Value1"},
                    {"Tag2", "Value2"}
                },
                Logs = new List<LogInfo>
                {
                    new LogInfo
                    {
                        Time = DateTimeOffset.Now,
                        Contents = new Dictionary<String, String>
                        {
                            {"foo", "bar"},
                            {"far", "baz"}
                        }
                    }
                }
            });
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(nameof(WaitLogStorePrepared))]
        public async Task TestPostLogStoreLogsWithHashKey()
        {
            var response = await this.context.Client.PostLogStoreLogsAsync(
                this.context.LogStoreName,
                new LogGroupInfo
                {
                    Topic = "UnitTest",
                    Source = "UnitTest",
                    LogTags = new Dictionary<String, String>
                    {
                        {"Tag1", "Value1"},
                        {"Tag2", "Value2"}
                    },
                    Logs = new List<LogInfo>
                    {
                        new LogInfo
                        {
                            Time = DateTimeOffset.Now,
                            Contents = new Dictionary<String, String>
                            {
                                {"foo", "bar"},
                                {"far", "baz"}
                            }
                        }
                    }
                },
                hashKey: "0123456789abcdef0123456789abcdef");
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(nameof(TestGetCursor))]
        public async Task TestPullLogs()
        {
            var response = await this.context.Client.PullLogsAsync(this.context.LogStoreName, this.context.WholeShardId, this.context.Cursor, 10);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.output.WriteLine("================================");
            this.output.WriteLine("Known headers:");
            this.output.WriteLine($"{LogHeaders.Count}={response.GetLogCount()}");
            this.output.WriteLine($"{LogHeaders.BodyRawSize}={response.GetLogBodyRawSize()}");
            this.output.WriteLine($"{LogHeaders.CompressType}={response.GetLogCompressType()}");
            this.output.WriteLine($"{LogHeaders.Cursor}={response.GetLogCursor()}");
        }

        [Fact, TestPriority(nameof(TestPostLogStoreLogs))]
        public async Task TestGetLogs()
        {
            var retryCount = new Int32[1];

            async Task<IResponse<GetLogsResult>> Run()
            {
                retryCount[0] = retryCount[0] + 1;
                return await this.context.Client.GetLogsAsync(
                    this.context.LogStoreName,
                    DateTimeOffset.UtcNow.AddMinutes(-1),
                    DateTimeOffset.UtcNow,
                    offset: 0,
                    line: 10);
            }

            var response = await Run();
            // Wait index created.
            while (response.GetHttpStatusCode() == HttpStatusCode.BadRequest && retryCount[0] < 12)
            {
                response = await Run();
                await Task.Delay(TimeSpan.FromSeconds(10));
            }

            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.output.WriteLine("================================");
            this.output.WriteLine("Known headers:");
            this.output.WriteLine($"{LogHeaders.Count}={response.GetLogCount()}");
            this.output.WriteLine($"{LogHeaders.ElapsedMillisecond}={response.GetLogElapsedMillisecond()}");
            this.output.WriteLine($"{LogHeaders.ProcessedRows}={response.GetLogProcessedRows()}");
            this.output.WriteLine($"{LogHeaders.Progress}={response.GetLogProgress()}");
            this.output.WriteLine($"{LogHeaders.HasSql}={response.GetHasSql()}");
            this.output.WriteLine($"{LogHeaders.QueryInfo}(String)={response.GetQueryInfoAsString()}");
            this.output.WriteLine($"{LogHeaders.QueryInfo}(Dictionary)={response.GetQueryInfoAsDictionary()}");
            this.output.WriteLine($"{LogHeaders.QueryInfo}(Dynamic)={response.GetQueryInfoAsDynamic()}");
            this.output.WriteLine($"{LogHeaders.AggQuery}={response.GetAggQuery()}");
            this.output.WriteLine($"{LogHeaders.WhereQuery}={response.GetWhereQuery()}");
        }

        [Fact, TestPriority(nameof(TestGetLogStore), nameof(TestPostLogStoreLogs))]
        public async Task TestGetProjectLogs()
        {
            var response = await this.context.Client.GetProjectLogsAsync(
                $"select count(*) from {this.context.LogStoreName}");
            this.PrintResponse(response, true);
            Assert.True(response.IsSuccess);

            this.output.WriteLine("================================");
            this.output.WriteLine("Known headers:");
            this.output.WriteLine($"{LogHeaders.Count}={response.GetLogCount()}");
            this.output.WriteLine($"{LogHeaders.ElapsedMillisecond}={response.GetLogElapsedMillisecond()}");
            this.output.WriteLine($"{LogHeaders.ProcessedRows}={response.GetLogProcessedRows()}");
            this.output.WriteLine($"{LogHeaders.Progress}={response.GetLogProgress()}");
            this.output.WriteLine($"{LogHeaders.HasSql}={response.GetHasSql()}");
            this.output.WriteLine($"{LogHeaders.QueryInfo}(String)={response.GetQueryInfoAsString()}");
            this.output.WriteLine($"{LogHeaders.QueryInfo}(Dictionary)={response.GetQueryInfoAsDictionary()}");
            this.output.WriteLine($"{LogHeaders.QueryInfo}(Dynamic)={response.GetQueryInfoAsDynamic()}");
            this.output.WriteLine($"{LogHeaders.AggQuery}={response.GetAggQuery()}");
            this.output.WriteLine($"{LogHeaders.WhereQuery}={response.GetWhereQuery()}");
        }

        [Fact, TestPriority(nameof(TestGetLogStore), nameof(TestPostLogStoreLogs))]
        public async Task TestGetHistograms()
        {
            var response = await this.context.Client.GetHistogramsAsync(
                this.context.LogStoreName,
                DateTimeOffset.UtcNow.AddMinutes(-1),
                DateTimeOffset.UtcNow);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);

            this.output.WriteLine("================================");
            this.output.WriteLine("Known headers:");
            this.output.WriteLine($"{LogHeaders.Count}={response.GetLogCount()}");
            this.output.WriteLine($"{LogHeaders.Progress}={response.GetLogProgress()}");
        }

        #endregion Log

        #region MachineGroup

        [Fact, TestPriority(Int32.MinValue, nameof(TestCreateProject))]
        public async Task TestCreateMachineGroup()
        {
            if (!this.context.ShouldInit)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.CreateMachineGroupAsync(
                this.context.MachineGroupName,
                "ip",
                new[] {"1.2.3.4", "8.8.8.8"});
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(Int32.MaxValue, nameof(TestRemoveConfigFromMachineGroup))]
        public async Task TestDeleteMachineGroup()
        {
            if (!this.context.ShouldClean)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.DeleteMachineGroupAsync(
                this.context.MachineGroupName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestUpdateMachineGroup()
        {
            var response = await this.context.Client.UpdateMachineGroupAsync(
                this.context.MachineGroupName,
                "ip",
                new[] {"1.2.3.4"});
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestListMachineGroup()
        {
            var response = await this.context.Client.ListMachineGroupAsync();
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestGetMachineGroup()
        {
            var response = await this.context.Client.GetMachineGroupAsync(
                this.context.MachineGroupName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestApplyConfigToMachineGroup()
        {
            var response = await this.context.Client.ApplyConfigToMachineGroupAsync(
                this.context.MachineGroupName,
                this.context.ConfigName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(Int32.MaxValue)]
        public async Task TestRemoveConfigFromMachineGroup()
        {
            var response = await this.context.Client.RemoveConfigFromMachineGroupAsync(
                this.context.MachineGroupName,
                this.context.ConfigName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestListMachines()
        {
            var response = await this.context.Client.ListMachinesAsync(
                this.context.MachineGroupName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(nameof(TestApplyConfigToMachineGroup))]
        public async Task TestGetAppliedConfigs()
        {
            var response = await this.context.Client.GetAppliedConfigsAsync(
                this.context.MachineGroupName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        #endregion MachineGroup

        #region Config

        [Fact, TestPriority(Int32.MinValue, nameof(TestCreateLogStore))]
        public async Task TestCreateConfig()
        {
            if (!this.context.ShouldInit)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.CreateFileToLogServiceConfigAsync(
                configName: this.context.ConfigName,
                logstoreName: this.context.LogStoreName,
                logType: "common_reg_log",
                logPath: "/var/logs/",
                filePattern: "access*.log",
                localStorage: false,
                timeFormat: "%Y/%m/%d %H:%M:%S",
                logBeginRegex: @"\d+\.\d+\.\d+\.\d+ - .*",
                regex: @"([\d\.]+) \S+ \S+ \[(\S+) \S+\] ""(\w+) ([^""]*)"" ([\d\.]+) (\d+) (\d+) (\d+|-) ""([^""]*)"" ""([^""]*)"".*",
                key: new[] {"ip", "time", "method", "url", "request_time", "request_length", "status", "length", "ref_url", "browser"},
                filterKey: Enumerable.Empty<String>(),
                filterRegex: Enumerable.Empty<String>(),
                topicFormat: "none",
                fileEncoding: "utf8");
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestListConfig()
        {
            var response = await this.context.Client.ListConfigAsync();
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(nameof(TestApplyConfigToMachineGroup))]
        public async Task TestGetAppliedMachineGroups()
        {
            var response = await this.context.Client.GetAppliedMachineGroupsAsync(this.context.ConfigName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestGetConfig()
        {
            var response = await this.context.Client.GetConfigAsync(this.context.ConfigName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(Int32.MaxValue, nameof(TestRemoveConfigFromMachineGroup))]
        public async Task TestDeleteConfig()
        {
            if (!this.context.ShouldClean)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.DeleteConfigAsync(this.context.ConfigName);
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestUpdateConfig()
        {
            var response = await this.context.Client.UpdateConfigAsync(
                this.context.ConfigName,
                "file",
                new ConfigInputDetailInfo(
                    logType: "common_reg_log",
                    logPath: "/var/logs/",
                    filePattern: "access*.log",
                    localStorage: false,
                    timeFormat: "%Y/%m/%d %H:%M:%S",
                    logBeginRegex: @"\d+\.\d+\.\d+\.\d+ - .*",
                    regex: @"([\d\.]+) \S+ \S+ \[(\S+) \S+\] ""(\w+) ([^""]*)"" ([\d\.]+) (\d+) (\d+) (\d+|-) ""([^""]*)"" ""([^""]*)"".*",
                    key: new[] {"ip", "time", "method", "url", "request_time", "request_length", "status", "length", "ref_url", "browser"},
                    filterKey: Enumerable.Empty<String>(),
                    filterRegex: Enumerable.Empty<String>())
                {
                    TopicFormat = "none",
                    FileEncoding = "utf8",
                    Preserve = false
                },
                "LogService",
                new ConfigOutputDetailInfo(this.context.LogStoreName));
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        #endregion Config

        #region Project

        [Fact, TestPriority(Int32.MinValue)]
        public async Task TestCreateProject()
        {
            if (!this.context.ShouldInitProject)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.CreateProjectAsync(this.context.ProjectName, "project description");
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        [Fact]
        public async Task TestListProject()
        {
            var response = await this.context.Client.ListProjectAsync();
            this.PrintResponse(response, true);
            Assert.True(response.IsSuccess);
            Assert.NotEmpty(response.Result.Projects);
        }

        [Fact]
        public async Task TestGetProject()
        {
            var response = await this.context.Client.GetProjectAsync();
            this.PrintResponse(response, true);
            Assert.True(response.IsSuccess);
        }

        [Fact, TestPriority(Int32.MaxValue, nameof(TestDeleteLogStore), nameof(TestDeleteConfig), nameof(TestDeleteMachineGroup))]
        public async Task TestDeleteProject()
        {
            if (!this.context.ShouldCleanProject)
            {
                this.output.WriteLine("Test is skipped!");
                return;
            }

            var response = await this.context.Client.DeleteProjectAsync();
            this.PrintResponse(response);
            Assert.True(response.IsSuccess);
        }

        #endregion
    }
}
