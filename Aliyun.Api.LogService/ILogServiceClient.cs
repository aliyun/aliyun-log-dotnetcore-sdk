//
// ILogServiceClient.cs
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain.Config;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Domain.LogStore;
using Aliyun.Api.LogService.Domain.LogStore.Index;
using Aliyun.Api.LogService.Domain.LogStore.Shard;
using Aliyun.Api.LogService.Domain.LogStore.Shipper;
using Aliyun.Api.LogService.Domain.MachineGroup;
using Aliyun.Api.LogService.Domain.Project;
using Aliyun.Api.LogService.Infrastructure.Protocol;

namespace Aliyun.Api.LogService
{
    public interface ILogServiceClient
    {
        #region LogStore

        /// <summary>
        /// 在 Project 下创建 Logstore。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> CreateLogStoreAsync(CreateLogStoreRequest request);

        /// <summary>
        /// 删除 Logstore，包括所有 Shard 数据，以及索引等。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> DeleteLogStoreAsync(DeleteLogStoreRequest request);

        /// <summary>
        /// 更新 Logstore 的属性。目前只支持更新 TTL和shard 属性。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> UpdateLogStoreAsync(UpdateLogStoreRequest request);

        /// <summary>
        /// 查看 Logstore 属性。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetLogStoreResult>> GetLogStoreAsync(GetLogStoreRequest request);

        /// <summary>
        /// 接口列出指定 Project 下的所有 Logstore 的名称。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<ListLogStoreResult>> ListLogStoreAsync(ListLogStoreRequest request);

        #region Shard

        /// <summary>
        /// 列出 Logstore 下当前所有可用 Shard。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<IList<ShardInfo>>> ListShardsAsync(ListShardRequest request);

        /// <summary>
        /// 分裂一个指定的 readwrite 状态的 Shard。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<IList<ShardInfo>>> SplitShardAsync(SplitShardRequest request);

        /// <summary>
        /// 合并两个相邻的 readwrite 状态的 Shards。在参数中指定一个 shardid，服务端自动找相邻的下一个 Shard。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<IList<ShardInfo>>> MergeShardsAsync(MergeShardRequest request);

        /// <summary>
        /// 根据时间获得游标（cursor）。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetCursorResult>> GetCursorAsync(GetCursorRequest request);

        #endregion Shard

        #region Shipper

        /// <summary>
        /// 查询日志投递任务状态。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetShipperResult>> GetShipperStatusAsync(GetShipperRequest request);

        /// <summary>
        /// 重新执行失败的日志投递任务。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> RetryShipperTaskAsync(RetryShipperRequest request);

        #endregion Shipper

        #region Index

        /// <summary>
        /// 开启日志库索引。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> CreateIndexAsync(CreateIndexRequest request);

        #endregion Index

        #endregion LogStore

        #region Logs

        /// <summary>
        /// 向指定的 LogStore 写入日志数据。目前仅支持写入 PB 格式的 <see cref="LogGroupInfo"/> 日志数据。写入时有两种模式：
        /// <list type="bullet">
        ///   <item>
        ///     <description>负载均衡模式（LoadBalance）: 自动根据 Logstore 下所有可写的 shard 进行负载均衡写入。该方法对写入可用性较高（SLA: 99.95%），适合写入与消费数据与 shard 无关的场景，例如不保序。</description>
        ///   </item>
        ///   <item>
        ///     <description>根据 Key 路由 shard 模式（KeyHash）：写入时需要传递一个 Key，服务端自动根据 Key 选择当前符合该 Key 区间的 Shard 写入。例如，可以将某个生产者（例如 instance）根据名称 Hash 到固定 Shard 上，这样就能保证写入与消费在该 Shard 上是严格有序的（在 Merge/Split 过程中能够严格保证对于 Key 在一个时间点只会出现在一个 Shard 上，参见 <see cref="http://help.aliyun.com/document_detail/28976.html">shard 数据模型</see>）。</description>
        ///   </item>
        /// </list>
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> PostLogStoreLogsAsync(PostLogsRequest request);

        /// <summary>
        /// 根据游标、数量获得日志。获得日志时必须指定 shard。
        /// 如果在 storm 等情况下可以通过 LoghubClientLib 进行选举与协同消费。
        /// 目前仅支持读取 PB 格式 LogGroupList 数据。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<PullLogsResult>> PullLogsAsync(PullLogsRequest request);

        /// <summary>
        /// GetLogs 接口查询指定 Project 下某个 Logstore 中的日志数据。还可以通过指定相关参数仅查询符合指定条件的日志数据。

        /// 当日志写入到 Logstore 中，日志服务的查询接口（GetHistograms 和 GetLogs）能够查到该日志的延时因写入日志类型不同而异。日志服务按日志时间戳把日志分为如下两类：
        /// <list type="bullet">
        ///   <item><description>实时数据：日志中时间点为服务器当前时间点 (-180秒，900秒]。例如，日志时间为 UTC 2014-09-25 12:03:00，服务器收到时为 UTC 2014-09-25 12:05:00，则该日志被作为实时数据处理，一般出现在正常场景下。</description></item>
        ///   <item><description>历史数据：日志中时间点为服务器当前时间点 [-7 x 86400秒, -180秒)。例如，日志时间为 UTC 2014-09-25 12:00:00，服务器收到时为 UTC 2014-09-25 12:05:00，则该日志被作为历史数据处理，一般出现在补数据场景下。</description></item>
        /// </list>
        ///
        /// 其中，实时数据写入至可查询的最大延时为3秒（99.9%情况下1秒内即可查询）。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetLogsResult>> GetLogsAsync(GetLogsRequest request);
        
        /// <summary>
        /// 统计Project下所有日志。
        /// <list type="bullet">
        ///   <item><description>该接口的query是一个标准的SQL查询语句。</description></item>
        ///   <item><description>查询的Project，在请求的域名中指定。</description></item>
        ///   <item><description>查询的logstore，在查询语句的from条件中指定。logstore相当于SQL中的表。</description></item>
        ///   <item><description>在查询的SQL条件中，必须指定要查询的时间范围，时间范围由__date__(timestamp类型)来指定，或__time__(int 类型，单位是unix_time)来指定。</description></item>
        ///   <item><description>如上所述，该接口一次调用必须要在限定时间内返回结果，每次查询只能扫描指定条数的日志量。如果一次请求需要处理的数据量非常大的时候，该请求会返回不完整的结果（并在返回结果中的 x-log-progress 成员标示是否完整）。如此同时，服务端会缓存 15 分钟内的查询结果。当查询请求的结果有部分被缓存命中，则服务端会在这次请求中继续扫描未被缓存命中的日志数据。为了减少您合并多次查询结果的工作量，服务端会把缓存命中的查询结果与本次查询新命中的结果合并返回给您。因此，日志服务可以让您通过以相同参数反复调用该接口来获取最终完整结果。因为您的查询涉及的日志数据量变化非常大，日志服务 API 无法预测需要调用多少次该接口而获取完整结果。所以需要用户通过检查每次请求的返回结果中的x-log-progress成员状态值来确定是否需要继续。需要注意的是，每次重复调用该接口都会重新消耗相同数量的查询 CU。</description></item>
        /// </list>
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetLogsResult>> GetProjectLogsAsync(GetProjectLogsRequest request);

        /// <summary>
        /// GetHistograms 接口查询指定的 Project 下某个 Logstore 中日志的分布情况。还可以通过指定相关参数仅查询符合指定条件的日志分布情况。
        /// 当日志写入到 logstore 中，日志服务的查询接口（GetHistograms 和 GetLogs）能够查到该日志的延时因写入日志类型不同而异。日志服务按日志时间戳把日志分为如下两类：
        /// <list type="bullet">
        ///   <item><description>实时数据：日志中时间点为服务器当前时间点 (-180秒，900秒]。例如，日志时间为 UTC 2014-09-25 12:03:00，服务器收到时为 UTC 2014-09-25 12:05:00，则该日志被作为实时数据处理，一般出现在正常场景下。</description></item>
        ///   <item><description>历史数据：日志中时间点为服务器当前时间点 [-7 x 86400秒, -180秒)。例如，日志时间为 UTC 2014-09-25 12:00:00，服务器收到时为 UTC 2014-09-25 12:05:00，则该日志被作为历史数据处理，一般出现在补数据场景下。</description></item>
        /// </list>
        /// 
        /// 其中，实时数据写入至可查询的延时为3秒。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetLogHistogramsResult>> GetHistogramsAsync(GetLogHistogramsRequest request);

        #endregion Logs
        
        #region MachineGroup

        /// <summary>
        /// 根据需求创建一组机器，用以日志收集下发配置。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> CreateMachineGroupAsync(CreateMachineGroupRequest request);

        /// <summary>
        /// 删除机器组，如果机器组上有配置，则 Logtail 上对应的配置也会被删除。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> DeleteMachineGroupAsync(DeleteMachineGroupRequest request);

        /// <summary>
        /// 更新机器组信息，如果机器组已应用配置，则新加入、减少机器会自动增加、移除配置。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> UpdateMachineGroupAsync(UpdateMachineGroupRequest request);

        /// <summary>
        /// 列出 MachineGroup 信息。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<ListMachineGroupResult>> ListMachineGroupAsync(ListMachineGroupRequest request);

        /// <summary>
        /// 查看具体的 MachineGroup 信息。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetMachineGroupResult>> GetMachineGroupAsync(GetMachineGroupRequest request);

        /// <summary>
        /// 将配置应用到机器组。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> ApplyConfigToMachineGroupAsync(ApplyConfigToMachineGroupRequest request);

        /// <summary>
        /// 从机器组中删除配置。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> RemoveConfigFromMachineGroupAsync(RemoveConfigFromMachineGroupRequest request);

        /// <summary>
        /// 获得 MachineGroup 下属于用户并与 Server 端连接的机器状态信息。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<ListMachinesResult>> ListMachinesAsync(ListMachinesRequest request);

        /// <summary>
        /// 获得 MachineGroup 上已经被应用的配置名称。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetAppliedConfigsResult>> GetAppliedConfigsAsync(GetAppliedConfigsRequest request);

        #endregion MachineGroup

        #region Config

        /// <summary>
        /// 在 Project 下创建日志配置。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> CreateConfigAsync(CreateConfigRequest request);

        /// <summary>
        /// 列出 Project 下所有配置信息，可以通过参数进行翻页。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<ListConfigResult>> ListConfigAsync(ListConfigRequest request);

        /// <summary>
        /// 列出 config 应用的机器列表。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetAppliedMachineGroupsResult>> GetAppliedMachineGroupsAsync(GetAppliedMachineGroupsRequest request);

        /// <summary>
        /// 获得一个配置的详细信息。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetConfigResult>> GetConfigAsync(GetConfigRequest request);

        /// <summary>
        /// 删除特定 config，如果 config 已被 应用到机器组，则 Logtail 配置也会被删除。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> DeleteConfigAsync(DeleteConfigRequest request);

        /// <summary>
        /// 更新配置内容，如果配置被应用到机器组，对应机器也会同时更新。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> UpdateConfigAsync(UpdateConfigRequest request);

        #endregion Config

        #region Project

        /// <summary>
        /// 创建project。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> CreateProjectAsync(CreateProjectRequest request);

        /// <summary>
        /// 获取指定区域Project列表。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<ListProjectResult>> ListProjectAsync(ListProjectRequest request);

        /// <summary>
        /// 获取当前Project信息。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse<GetProjectResult>> GetProjectAsync(GetProjectRequest request);
        
        /// <summary>
        /// 删除当前Project。
        /// </summary>
        /// <param name="request">请求报文。</param>
        /// <returns>异步响应结果。</returns>
        Task<IResponse> DeleteProjectAsync(DeleteProjectRequest request);
        
        #endregion
    }
}
