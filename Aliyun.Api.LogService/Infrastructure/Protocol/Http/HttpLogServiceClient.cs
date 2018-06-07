//
// HttpLogServiceClient.cs
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
using System.Net.Http;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain.Config;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Domain.LogStore;
using Aliyun.Api.LogService.Domain.LogStore.Index;
using Aliyun.Api.LogService.Domain.LogStore.Shard;
using Aliyun.Api.LogService.Domain.LogStore.Shipper;
using Aliyun.Api.LogService.Domain.MachineGroup;
using Aliyun.Api.LogService.Domain.Project;
using Aliyun.Api.LogService.Infrastructure.Authentication;
using Aliyun.Api.LogService.Infrastructure.Serialization.Protobuf;
using Aliyun.Api.LogService.Utils;
using Newtonsoft.Json;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    public class HttpLogServiceClient : ILogServiceClient
    {
        private readonly HttpClient httpClient;

        private readonly Func<Credential> credentialProvider;

        public HttpLogServiceClient(HttpClient httpClient, Func<Credential> credentialProvider)
        {
            this.httpClient = httpClient;
            this.credentialProvider = credentialProvider;
        }


        #region Helper

        private async Task<TResponse> SendRequestAsync<TResponse>(IRequestBuilder<HttpRequestMessage> requestBuilder,
            Func<IResponseResolver, Task<TResponse>> resposneResolver, Boolean outOfProject = false, String project = null)
        {
            var credential = this.credentialProvider();

            var httpRequestMessage = requestBuilder
                .Authenticate(credential)
                .Sign(SignatureType.HmacSha1)
                .Build();

            if (outOfProject)
            {
                httpRequestMessage.Headers.Host = this.httpClient.BaseAddress.Host;
            } else if (project.IsNotEmpty())
            {
                httpRequestMessage.Headers.Host = $"{project}.{this.httpClient.BaseAddress.Host}";
            }

            var responseMessage = await this.httpClient.SendAsync(httpRequestMessage);
            return await resposneResolver(HttpResponseMessageResolver.For(responseMessage));
        }

        private Task<IResponse> SendRequestAsync(IRequestBuilder<HttpRequestMessage> requestBuilder, Boolean outOfProject = false, String project = null)
            => this.SendRequestAsync(requestBuilder, x => x.ResolveAsync(), outOfProject, project);

        private Task<IResponse<TResult>> SendRequestAsync<TResult>(IRequestBuilder<HttpRequestMessage> requestBuilder, Boolean outOfProject = false, String project = null)
            where TResult : class
            => this.SendRequestAsync(requestBuilder, x => x.With<TResult>().ResolveAsync(), outOfProject, project);

        #endregion


        #region LogStore

        public Task<IResponse> CreateLogStoreAsync(CreateLogStoreRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Post, "/logstores")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        public Task<IResponse> DeleteLogStoreAsync(DeleteLogStoreRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Delete, $"/logstores/{request.LogstoreName}"),
                project:request.ProjectName);

        public Task<IResponse> UpdateLogStoreAsync(UpdateLogStoreRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Put, $"/logstores/{request.LogstoreName}")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        public Task<IResponse<GetLogStoreResult>> GetLogStoreAsync(GetLogStoreRequest request)
            => this.SendRequestAsync<GetLogStoreResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/logstores/{request.LogstoreName}"),
                project:request.ProjectName);

        public Task<IResponse<ListLogStoreResult>> ListLogStoreAsync(ListLogStoreRequest request)
            => this.SendRequestAsync<ListLogStoreResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, "/logstores")
                    .Query(request),
                project:request.ProjectName);

        #region Shard

        public Task<IResponse<IList<ShardInfo>>> ListShardsAsync(ListShardRequest request)
            => this.SendRequestAsync<IList<ShardInfo>>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/logstores/{request.LogstoreName}/shards"),
                project:request.ProjectName);

        public Task<IResponse<IList<ShardInfo>>> SplitShardAsync(SplitShardRequest request)
            => this.SendRequestAsync<IList<ShardInfo>>(
                new HttpRequestMessageBuilder(HttpMethod.Post, $"/logstores/{request.LogstoreName}/shards/{request.ShardId}")
                    .Query("action", "split")
                    .Query("key", request.SplitKey),
                project:request.ProjectName);

        public Task<IResponse<IList<ShardInfo>>> MergeShardsAsync(MergeShardRequest request)
            => this.SendRequestAsync<IList<ShardInfo>>(
                new HttpRequestMessageBuilder(HttpMethod.Post, $"/logstores/{request.LogstoreName}/shards/{request.ShardId}")
                    .Query("action", "merge"),
                project:request.ProjectName);

        public Task<IResponse<GetCursorResult>> GetCursorAsync(GetCursorRequest request)
            => this.SendRequestAsync<GetCursorResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/logstores/{request.LogstoreName}/shards/{request.ShardId}")
                    .Query("type", "cursor")
                    .Query("from", request.From),
                project:request.ProjectName);

        #endregion Shard

        #region Shipper

        public Task<IResponse<GetShipperResult>> GetShipperStatusAsync(GetShipperRequest request)
            => this.SendRequestAsync<GetShipperResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/logstores/{request.LogstoreName}/shipper/{request.ShipperName}/tasks")
                    .Query("from", request.From.ToString())
                    .Query("to", request.To.ToString())
                    .Query("status", request.Status)
                    .Query("offset", request.Offset.ToString())
                    .Query("size", request.Size.ToString()),
                project:request.ProjectName);

        public Task<IResponse> RetryShipperTaskAsync(RetryShipperRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Put, $"/logstores/{request.LogstoreName}/shipper/{request.ShipperName}/tasks")
                    .Content(request.TaskIds)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        #endregion Shipper

        #region Index

        public Task<IResponse> CreateIndexAsync(CreateIndexRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Post, $"/logstores/{request.LogstoreName}/index")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        #endregion Index

        #endregion LogStore


        #region Logs

        public Task<IResponse> PostLogStoreLogsAsync(PostLogsRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Post, request.HashKey.IsEmpty()
                        ? $"/logstores/{request.LogstoreName}/shards/lb"
                        : $"/logstores/{request.LogstoreName}/shards/route?key={request.HashKey}")
                    .Content(request.LogGroup.ToProtoModel())
                    .Serialize(SerializeType.Protobuf)
                    .Compress(CompressType.Lz4),
                project:request.ProjectName);

        public Task<IResponse<PullLogsResult>> PullLogsAsync(PullLogsRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/logstores/{request.LogstoreName}/shards/{request.ShardId}")
                    .Query("type", "logs")
                    .Query("cursor", request.Cursor)
                    .Query("count", request.Count.ToString())
                    .Header("Accept", "application/x-protobuf")
                    .Header("Accept-Encoding", "lz4"),
                resolver => resolver
                    .Deserialize(x => LogGroupList.Parser.ParseFrom(x))
                    .ResolveAsync(x => new PullLogsResult(x.ToDomainModel())),
                project:request.ProjectName);

        public async Task<IResponse<GetLogsResult>> GetLogsAsync(GetLogsRequest request)
            => (await this.SendRequestAsync<IList<IDictionary<String, String>>>(
                    new HttpRequestMessageBuilder(HttpMethod.Get, $"/logstores/{request.Logstorename}")
                        .Query("type", "log")
                        .Query("from", request.From.ToString())
                        .Query("to", request.To.ToString())
                        .Query("topic", request.Topic)
                        .Query("query", request.Query)
                        .Query("line", request.Line.ToString())
                        .Query("offset", request.Offset.ToString())
                        .Query("reverse", request.Reverse.ToString()),
                    project:request.ProjectName))
                .Transform((httpHeaders, data) =>
                {
                    var newResult = new GetLogsResult(data)
                    {
                        Count = Convert.ToInt32(httpHeaders.GetValueOrDefault(LogHeaders.Count)),
                        ProcessedRows = Convert.ToInt32(httpHeaders.GetValueOrDefault(LogHeaders.ProcessedRows)),
                        ElapsedMillisecond = Convert.ToInt32(httpHeaders.GetValueOrDefault(LogHeaders.ElapsedMillisecond)),
                        HasSql = Convert.ToBoolean(httpHeaders.GetValueOrDefault(LogHeaders.HasSql)),
                        AggQuery = httpHeaders.GetValueOrDefault(LogHeaders.AggQuery),
                        WhereQuery = httpHeaders.GetValueOrDefault(LogHeaders.WhereQuery)
                    };

                    // Parse Progress
                    if (Enum.TryParse<LogProgressState>(httpHeaders.GetValueOrDefault(LogHeaders.Progress), true, out var progress))
                    {
                        newResult.Progress = progress;
                    }

                    // Parse QueryInfo
                    if (httpHeaders.TryGetValue(LogHeaders.QueryInfo, out var queryInfoValue))
                    {
                        newResult.QueryInfo = JsonConvert.DeserializeObject<LogQueryInfo>(queryInfoValue);
                    }

                    return newResult;
                });

        public async Task<IResponse<GetLogsResult>> GetProjectLogsAsync(GetProjectLogsRequest request)
            => (await this.SendRequestAsync<IList<IDictionary<String, String>>>(
                    new HttpRequestMessageBuilder(HttpMethod.Get, "/logs")
                        .Query(request),
                    project:request.ProjectName))
                .Transform((httpHeaders, data) =>
                {
                    var newResult = new GetLogsResult(data)
                    {
                        Count = Convert.ToInt32(httpHeaders.GetValueOrDefault(LogHeaders.Count)),
                        ProcessedRows = Convert.ToInt32(httpHeaders.GetValueOrDefault(LogHeaders.ProcessedRows)),
                        ElapsedMillisecond = Convert.ToInt32(httpHeaders.GetValueOrDefault(LogHeaders.ElapsedMillisecond)),
                        HasSql = Convert.ToBoolean(httpHeaders.GetValueOrDefault(LogHeaders.HasSql)),
                        AggQuery = httpHeaders.GetValueOrDefault(LogHeaders.AggQuery),
                        WhereQuery = httpHeaders.GetValueOrDefault(LogHeaders.WhereQuery)
                    };

                    // Parse Progress
                    if (Enum.TryParse<LogProgressState>(httpHeaders.GetValueOrDefault(LogHeaders.Progress), true, out var progress))
                    {
                        newResult.Progress = progress;
                    }

                    // Parse QueryInfo
                    if (httpHeaders.TryGetValue(LogHeaders.QueryInfo, out var queryInfoValue))
                    {
                        newResult.QueryInfo = JsonConvert.DeserializeObject<LogQueryInfo>(queryInfoValue);
                    }

                    return newResult;
                });

        public async Task<IResponse<GetLogHistogramsResult>> GetHistogramsAsync(GetLogHistogramsRequest request)
            => (await this.SendRequestAsync<IList<LogHistogramInfo>>(
                    new HttpRequestMessageBuilder(HttpMethod.Get, $"/logstores/{request.Logstorename}")
                        .Query("type", "histogram")
                        .Query("from", request.From.ToString())
                        .Query("to", request.To.ToString())
                        .Query("topic", request.Topic)
                        .Query("query", request.Query),
                    project:request.ProjectName))
                .Transform((httpHeaders, data) =>
                {
                    var newResult = new GetLogHistogramsResult(data)
                    {
                        Count = Convert.ToInt32(httpHeaders.GetValueOrDefault(LogHeaders.Count))
                    };

                    // Parse Progress
                    if (Enum.TryParse<LogProgressState>(httpHeaders.GetValueOrDefault(LogHeaders.Progress), true, out var progress))
                    {
                        newResult.Progress = progress;
                    }

                    return newResult;
                });

        #endregion Logs


        #region MachineGroup

        public Task<IResponse> CreateMachineGroupAsync(CreateMachineGroupRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Post, "/machinegroups")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        public Task<IResponse> DeleteMachineGroupAsync(DeleteMachineGroupRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Delete, $"/machinegroups/{request.GroupName}"),
                project:request.ProjectName);

        public Task<IResponse> UpdateMachineGroupAsync(UpdateMachineGroupRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Put, $"/machinegroups/{request.GroupName}")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        public Task<IResponse<ListMachineGroupResult>> ListMachineGroupAsync(ListMachineGroupRequest request)
            => this.SendRequestAsync<ListMachineGroupResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, "/machinegroups")
                    .Query(request),
                project:request.ProjectName);

        public Task<IResponse<GetMachineGroupResult>> GetMachineGroupAsync(GetMachineGroupRequest request)
            => this.SendRequestAsync<GetMachineGroupResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/machinegroups/{request.GroupName}"));

        public Task<IResponse> ApplyConfigToMachineGroupAsync(ApplyConfigToMachineGroupRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Put, $"/machinegroups/{request.GroupName}/configs/{request.ConfigName}"),
                project:request.ProjectName);

        public Task<IResponse> RemoveConfigFromMachineGroupAsync(RemoveConfigFromMachineGroupRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Delete, $"/machinegroups/{request.GroupName}/configs/{request.ConfigName}"),
                project:request.ProjectName);

        public Task<IResponse<ListMachinesResult>> ListMachinesAsync(ListMachinesRequest request)
            => this.SendRequestAsync<ListMachinesResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/machinegroups/{request.GroupName}/machines")
                    .Query("offset", request.Offset.ToString())
                    .Query("size", request.Size.ToString()),
                project:request.ProjectName);

        public Task<IResponse<GetAppliedConfigsResult>> GetAppliedConfigsAsync(GetAppliedConfigsRequest request)
            => this.SendRequestAsync<GetAppliedConfigsResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/machinegroups/{request.GroupName}/configs"),
                project:request.ProjectName);

        #endregion MachineGroup


        #region Config

        public Task<IResponse> CreateConfigAsync(CreateConfigRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Post, "/configs")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        public Task<IResponse<ListConfigResult>> ListConfigAsync(ListConfigRequest request)
            => this.SendRequestAsync<ListConfigResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, "/configs")
                    .Query(request),
                project:request.ProjectName);

        public Task<IResponse<GetAppliedMachineGroupsResult>> GetAppliedMachineGroupsAsync(GetAppliedMachineGroupsRequest request)
            => this.SendRequestAsync<GetAppliedMachineGroupsResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/configs/{request.ConfigName}/machinegroups"),
                project:request.ProjectName);

        public Task<IResponse<GetConfigResult>> GetConfigAsync(GetConfigRequest request)
            => this.SendRequestAsync<GetConfigResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, $"/configs/{request.ConfigName}"),
                project:request.ProjectName);

        public Task<IResponse> DeleteConfigAsync(DeleteConfigRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Delete, $"/configs/{request.ConfigName}"),
                project:request.ProjectName);

        public Task<IResponse> UpdateConfigAsync(UpdateConfigRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Put, $"/configs/{request.ConfigName}")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                project:request.ProjectName);

        #endregion Config


        #region Project

        public Task<IResponse> CreateProjectAsync(CreateProjectRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Post, "/")
                    .Content(request)
                    .Serialize(SerializeType.Json),
                outOfProject: true);

        public Task<IResponse<ListProjectResult>> ListProjectAsync(ListProjectRequest request)
            => this.SendRequestAsync<ListProjectResult>(
                new HttpRequestMessageBuilder(HttpMethod.Get, "/")
                    .Query(request),
                outOfProject: true);

        public Task<IResponse<GetProjectResult>> GetProjectAsync(GetProjectRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Get, "/"),
                resolver => resolver
                    .With<ProjectDetailInfo>()
                    .ResolveAsync(x => new GetProjectResult(x)),
                project: request.ProjectName);

        public Task<IResponse> DeleteProjectAsync(DeleteProjectRequest request)
            => this.SendRequestAsync(
                new HttpRequestMessageBuilder(HttpMethod.Delete, "/"),
                project: request.ProjectName);

        #endregion Project
    }
}
