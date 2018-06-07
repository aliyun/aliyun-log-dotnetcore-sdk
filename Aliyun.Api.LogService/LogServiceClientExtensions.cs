//
// LogServiceClientExtensions.cs
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
using Aliyun.Api.LogService.Utils;

namespace Aliyun.Api.LogService
{
    public static class LogServiceClientExtensions
    {
        #region LogStore

        /// <summary>
        /// 在 Project 下创建 Logstore。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="ttl">数据的保存时间，单位为天，范围1~365（额外需求请提交工单）。</param>
        /// <param name="shardCount">该 Logstore 的 Shard 数量，单位为个，范围为 1~10。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateLogStoreAsync"/>
        public static Task<IResponse> CreateLogStoreAsync(this ILogServiceClient client,
            String logstoreName, Int32 ttl, Int32 shardCount,
            String project = null)
            => client.CreateLogStoreAsync(new CreateLogStoreRequest(logstoreName, ttl, shardCount)
            {
                ProjectName = project
            });

        /// <summary>
        /// 删除 Logstore，包括所有 Shard 数据，以及索引等。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.DeleteLogStoreAsync"/>
        public static Task<IResponse> DeleteLogStoreAsync(this ILogServiceClient client,
            String logstoreName,
            String project = null)
            => client.DeleteLogStoreAsync(new DeleteLogStoreRequest(logstoreName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 更新 Logstore 的属性。目前只支持更新 TTL和shard 属性。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="ttl">数据的保存时间，单位为天，范围1~365（额外需求请提交工单）。</param>
        /// <param name="shardCount">该 Logstore 的 Shard 数量，单位为个，范围为 1~10。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.UpdateLogStoreAsync"/>
        public static Task<IResponse> UpdateLogStoreAsync(this ILogServiceClient client,
            String logstoreName, Int32 ttl, Int32 shardCount,
            String project = null)
            => client.UpdateLogStoreAsync(new UpdateLogStoreRequest(logstoreName, ttl, shardCount)
            {
                ProjectName = project
            });

        /// <summary>
        /// 查看 Logstore 属性。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.UpdateLogStoreAsync"/>
        public static Task<IResponse<GetLogStoreResult>> GetLogStoreAsync(this ILogServiceClient client,
            String logstoreName,
            String project = null)
            => client.GetLogStoreAsync(new GetLogStoreRequest(logstoreName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 接口列出指定 Project 下的所有 Logstore 的名称。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">用于请求的 Logstore 名称（支持部分匹配）。</param>
        /// <param name="offset">返回记录的起始位置，默认值为 1。</param>
        /// <param name="size">每页返回最大条目，默认 500（最大值）。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.ListLogStoreAsync"/>
        public static Task<IResponse<ListLogStoreResult>> ListLogStoreAsync(this ILogServiceClient client,
            String logstoreName = null, Int32 offset = ListLogStoreRequest.DefaultOffset, Int32 size = ListLogStoreRequest.DefaultSize, String project = null)
            => client.ListLogStoreAsync(new ListLogStoreRequest
            {
                LogstoreName = logstoreName,
                Offset = offset,
                Size = size,
                ProjectName = project
            });

        #region Shard

        /// <summary>
        /// 列出 Logstore 下当前所有可用 Shard。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.ListShardsAsync"/>
        public static Task<IResponse<IList<ShardInfo>>> ListShardsAsync(this ILogServiceClient client,
            String logstoreName,
            String project = null)
            => client.ListShardsAsync(new ListShardRequest(logstoreName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 分裂一个指定的 readwrite 状态的 Shard。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称</param>
        /// <param name="shardId">Shard ID</param>
        /// <param name="splitKey">split 切分位置</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.SplitShardAsync"/>
        public static Task<IResponse<IList<ShardInfo>>> SplitShardAsync(this ILogServiceClient client,
            String logstoreName, Int32 shardId, String splitKey,
            String project = null)
            => client.SplitShardAsync(new SplitShardRequest(logstoreName, shardId, splitKey)
            {
                ProjectName = project
            });

        /// <summary>
        /// 合并两个相邻的 readwrite 状态的 Shards。在参数中指定一个 shardid，服务端自动找相邻的下一个 Shard。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称</param>
        /// <param name="shardId">Shard ID</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.MergeShardsAsync"/>
        public static Task<IResponse<IList<ShardInfo>>> MergeShardsAsync(this ILogServiceClient client,
            String logstoreName, Int32 shardId,
            String project = null)
            => client.MergeShardsAsync(new MergeShardRequest(logstoreName, shardId)
            {
                ProjectName = project
            });

        /// <summary>
        /// 根据时间获得游标（cursor）。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称</param>
        /// <param name="shardId">Shard ID</param>
        /// <param name="from">时间点（UNIX下秒数），或 begin，end</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetCursorAsync"/>
        public static Task<IResponse<GetCursorResult>> GetCursorAsync(this ILogServiceClient client,
            String logstoreName, Int32 shardId, String from,
            String project = null)
            => client.GetCursorAsync(new GetCursorRequest(logstoreName, shardId, from)
            {
                ProjectName = project
            });

        #endregion Shard

        #region Shipper

        /// <summary>
        /// 查询日志投递任务状态。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称，同一 Project 下唯一。</param>
        /// <param name="shipperName">日志投递规则名称，同一 Logstore 下唯一。</param>
        /// <param name="from">日志投递任务创建时间区间。</param>
        /// <param name="to">日志投递任务创建时间区间。</param>
        /// <param name="status">默认为空，表示返回所有状态的任务，目前支持 success/fail/running 等状态。</param>
        /// <param name="offset">返回指定时间区间内投递任务的起始数目，默认值为 0。</param>
        /// <param name="size">返回指定时间区间内投递任务的数目，默认值为 100，最大为 500。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetShipperStatusAsync"/>
        public static Task<IResponse<GetShipperResult>> GetShipperStatusAsync(this ILogServiceClient client,
            String logstoreName, String shipperName, DateTimeOffset from, DateTimeOffset to,
            String status = null, Int32 offset = GetShipperRequest.DefaultOffset, Int32 size = GetShipperRequest.DefaultSize, String project = null)
            => client.GetShipperStatusAsync(new GetShipperRequest(logstoreName, shipperName, (Int32)from.ToUnixTimeSeconds(), (Int32) to.ToUnixTimeSeconds())
            {
                Status = status,
                Offset = offset,
                Size = size,
                ProjectName = project
            });

        /// <summary>
        /// 重新执行失败的日志投递任务。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称，同一 Project 下唯一。</param>
        /// <param name="shipperName">日志投递规则名称，同一 Logstore 下唯一。</param>
        /// <param name="taskIds">需要重试的任务ID。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.RetryShipperTaskAsync"/>
        public static Task<IResponse> RetryShipperTaskAsync(this ILogServiceClient client,
            String logstoreName, String shipperName, IEnumerable<String> taskIds,
            String project = null)
            => client.RetryShipperTaskAsync(new RetryShipperRequest(logstoreName, shipperName, taskIds)
            {
                ProjectName = project
            });

        /// <summary>
        /// 重新执行失败的日志投递任务。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称，同一 Project 下唯一。</param>
        /// <param name="shipperName">日志投递规则名称，同一 Logstore 下唯一。</param>
        /// <param name="taskIds">需要重试的任务ID。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.RetryShipperTaskAsync"/>
        /// <remarks>
        /// 由于此方法存在变长参数，不支持使用可选参数覆盖 project，需要覆盖 project参数请使用
        /// <see cref="RetryShipperTaskAsync(Aliyun.Api.LogService.ILogServiceClient,string,string,System.Collections.Generic.IEnumerable{string},string)"/> 版本。
        /// </remarks>
        public static Task<IResponse> RetryShipperTaskAsync(this ILogServiceClient client,
            String logstoreName, String shipperName, params String[] taskIds)
            => client.RetryShipperTaskAsync(new RetryShipperRequest(logstoreName, shipperName, taskIds));

        #endregion Shipper

        #region Index

        /// <summary>
        /// 开启日志库索引。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="line">全文索引，对于日志中value的索引属性，全文索引和字段查询必须至少配置一类。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateIndexAsync"/>
        /// <exception cref="ArgumentException"><paramref cref="line"/> 为空</exception>
        public static Task<IResponse> CreateIndexAsync(this ILogServiceClient client,
            String logstoreName, IndexLineInfo line,
            String project = null)
        {
            Ensure.NotNull(line, nameof(line));

            return client.CreateIndexAsync(new CreateIndexRequest(logstoreName, line)
            {
                ProjectName = project
            });
        }

        /// <summary>
        /// 开启日志库索引。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="keys">字段查询，对于具体字段的value索引属性，全文索引和字段查询必须至少配置一类。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateIndexAsync"/>
        /// <exception cref="ArgumentException"><paramref name="keys"/> 为空</exception>
        public static Task<IResponse> CreateIndexAsync(this ILogServiceClient client,
            String logstoreName, IDictionary<String, IndexKeyInfo> keys,
            String project = null)
        {
            Ensure.NotEmpty(keys, nameof(keys));

            return client.CreateIndexAsync(new CreateIndexRequest(logstoreName, keys)
            {
                ProjectName = project
            });
        }

        /// <summary>
        /// 开启日志库索引。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="line">全文索引，对于日志中value的索引属性，全文索引和字段查询必须至少配置一类。</param>
        /// <param name="keys">字段查询，对于具体字段的value索引属性，全文索引和字段查询必须至少配置一类。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateIndexAsync"/>
        /// <exception cref="ArgumentException"><paramref cref="line"/> 和 <paramref name="keys"/> 同时为空</exception>
        public static Task<IResponse> CreateIndexAsync(this ILogServiceClient client,
            String logstoreName, IndexLineInfo line, IDictionary<String, IndexKeyInfo> keys,
            String project = null)
        {
            if (line == null && keys.IsEmpty())
            {
                throw new ArgumentException("line and keys cannot be both empty.");
            }

            return client.CreateIndexAsync(new CreateIndexRequest(logstoreName, line, keys)
            {
                ProjectName = project
            });
        }

        /// <summary>
        /// 开启日志库索引。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="keysBuilder">字段查询构建器。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateIndexAsync"/>
        /// <exception cref="ArgumentException"><paramref name="keysBuilder"/> 为空或没有任何keys被构建</exception>
        public static Task<IResponse> CreateIndexAsync(this ILogServiceClient client,
            String logstoreName, Action<IndexKeysBuilder> keysBuilder,
            String project = null)
        {
            IDictionary<String, IndexKeyInfo> keys;
            if (keysBuilder != null)
            {
                var builder = new IndexKeysBuilder();
                keysBuilder(builder);
                keys = builder.Build();
            } else
            {
                keys = null;
            }

            return client.CreateIndexAsync(logstoreName, keys, project);
        }

        /// <summary>
        /// 开启日志库索引。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">Logstore 的名称，在 Project 下必须唯一。</param>
        /// <param name="line">全文索引，对于日志中value的索引属性，全文索引和字段查询必须至少配置一类。</param>
        /// <param name="keysBuilder">字段查询构建器。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateIndexAsync"/>
        /// <exception cref="ArgumentException"><paramref cref="line"/> 和 <paramref name="keysBuilder"/> 同时为空</exception>
        public static Task<IResponse> CreateIndexAsync(this ILogServiceClient client,
            String logstoreName, IndexLineInfo line, Action<IndexKeysBuilder> keysBuilder,
            String project = null)
        {
            IDictionary<String, IndexKeyInfo> keys;
            if (keysBuilder != null)
            {
                var builder = new IndexKeysBuilder();
                keysBuilder(builder);
                keys = builder.Build();
            } else
            {
                keys = null;
            }

            return client.CreateIndexAsync(logstoreName, line, keys, project);
        }

        #endregion Index

        #endregion LogStore

        #region Log

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
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称。</param>
        /// <param name="logGroup">一组日志。</param>
        /// <param name="hashKey">（可选）标记日志应该路由到哪个 shard 的标记。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.PostLogStoreLogsAsync"/>
        public static Task<IResponse> PostLogStoreLogsAsync(this ILogServiceClient client,
            String logstoreName, LogGroupInfo logGroup,
            String hashKey = null, String project = null)
            => client.PostLogStoreLogsAsync(new PostLogsRequest(logstoreName, logGroup)
            {
                HashKey = hashKey,
                ProjectName = project
            });

        /// <summary>
        /// 根据游标、数量获得日志。获得日志时必须指定 shard。
        /// 如果在 storm 等情况下可以通过 LoghubClientLib 进行选举与协同消费。
        /// 目前仅支持读取 PB 格式 LogGroupList 数据。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="logstoreName">日志库名称</param>
        /// <param name="shardId">Shard ID</param>
        /// <param name="cursor">游标，用以表示从什么位置开始读取数据，相当于起点。</param>
        /// <param name="count">返回的 loggroup 数目，范围为 0~1000。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.PullLogsAsync"/>
        public static Task<IResponse<PullLogsResult>> PullLogsAsync(this ILogServiceClient client,
            String logstoreName, Int32 shardId, String cursor, Int32 count,
            String project = null)
            => client.PullLogsAsync(new PullLogsRequest(logstoreName, shardId, cursor, count)
            {
                ProjectName = project
            });

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
        /// <param name="client">client实例。</param>
        /// <param name="logstorename">需要查询日志的 Logstore 名称。</param>
        /// <param name="from">查询开始时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。</param>
        /// <param name="to">查询结束时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。</param>
        /// <param name="topic">查询日志主题。</param>
        /// <param name="query">查询表达式。关于查询表达式的详细语法，请参考 查询语法。</param>
        /// <param name="line">请求返回的最大日志条数。取值范围为 0~100，默认值为 100。</param>
        /// <param name="offset">请求返回日志的起始点。取值范围为 0 或正整数，默认值为 0。</param>
        /// <param name="reverse">是否按日志时间戳逆序返回日志。true 表示逆序，false 表示顺序，默认值为 false。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetLogsAsync"/>
        public static Task<IResponse<GetLogsResult>> GetLogsAsync(this ILogServiceClient client,
            String logstorename, DateTimeOffset from, DateTimeOffset to,
            String topic = null, String query = null, Int32 line = GetLogsRequest.DefaultLine, Int32 offset = GetLogsRequest.DefaultOffset, Boolean reverse = GetLogsRequest.DefaultReverse, String project = null)
            => client.GetLogsAsync(new GetLogsRequest(logstorename, (Int32)from.ToUnixTimeSeconds(), (Int32) to.ToUnixTimeSeconds())
            {
                Topic = topic,
                Query = query,
                Line = line,
                Offset = offset,
                Reverse = reverse,
                ProjectName = project
            });

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
        /// <param name="client">client实例。</param>
        /// <param name="query">查询sql条件。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetProjectLogsAsync"/>
        public static Task<IResponse<GetLogsResult>> GetProjectLogsAsync(this ILogServiceClient client,
            String query,
            String project = null)
            => client.GetProjectLogsAsync(new GetProjectLogsRequest(query)
            {
                ProjectName = project
            });

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
        /// <param name="client">client实例。</param>
        /// <param name="logstorename">需要查询日志的 Logstore 名称。</param>
        /// <param name="from">查询开始时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。</param>
        /// <param name="to">查询结束时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。</param>
        /// <param name="topic">查询日志主题。</param>
        /// <param name="query">查询表达式。关于查询表达式的详细语法，请参考 查询语法。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetHistogramsAsync"/>
        public static Task<IResponse<GetLogHistogramsResult>> GetHistogramsAsync(this ILogServiceClient client,
            String logstorename, DateTimeOffset from, DateTimeOffset to,
            String topic = null, String query = null, String project = null)
            => client.GetHistogramsAsync(new GetLogHistogramsRequest(logstorename, (Int32)from.ToUnixTimeSeconds(), (Int32) to.ToUnixTimeSeconds())
            {
                Topic = topic,
                Query = query,
                ProjectName = project
            });

        #endregion Log

        #region MachineGroup

        /// <summary>
        /// 根据需求创建一组机器，用以日志收集下发配置。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">机器分组名称。</param>
        /// <param name="machineIdentifyType">机器标识类型，分为 ip 和 userdefined 两种。</param>
        /// <param name="machineList">具体的机器标识，可以是 IP 或 userdefined-id。</param>
        /// <param name="groupType">机器分组类型，默认为空。</param>
        /// <param name="groupTopic">机器分组的 topic，默认为空。</param>
        /// <param name="externalName">机器分组所依赖的外部管理标识，默认为空。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateMachineGroupAsync"/>
        public static Task<IResponse> CreateMachineGroupAsync(this ILogServiceClient client,
            String groupName, String machineIdentifyType, IEnumerable<String> machineList,
            String groupType = null, String groupTopic = null, String externalName = null, String project = null)
            => client.CreateMachineGroupAsync(new CreateMachineGroupRequest(groupName, machineIdentifyType, machineList)
            {
                GroupType = groupType,
                GroupAttribute = groupTopic == null && externalName == null
                    ? null
                    : new MachineGroupAttributeInfo
                    {
                        GroupTopic = groupTopic,
                        ExternalName = externalName
                    },
                ProjectName = project
            });

        /// <summary>
        /// 删除机器组，如果机器组上有配置，则 Logtail 上对应的配置也会被删除。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">机器分组名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.DeleteMachineGroupAsync"/>
        public static Task<IResponse> DeleteMachineGroupAsync(this ILogServiceClient client,
            String groupName,
            String project = null)
            => client.DeleteMachineGroupAsync(new DeleteMachineGroupRequest(groupName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 更新机器组信息，如果机器组已应用配置，则新加入、减少机器会自动增加、移除配置。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">机器分组名称。</param>
        /// <param name="machineIdentifyType">机器标识类型，分为 ip 和 userdefined 两种。</param>
        /// <param name="machineList">具体的机器标识，可以是 IP 或 userdefined-id。</param>
        /// <param name="groupType">机器分组类型，默认为空。</param>
        /// <param name="groupTopic">机器分组的 topic，默认为空。</param>
        /// <param name="externalName">机器分组所依赖的外部管理标识，默认为空。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.UpdateMachineGroupAsync"/>
        public static Task<IResponse> UpdateMachineGroupAsync(this ILogServiceClient client,
            String groupName, String machineIdentifyType, IEnumerable<String> machineList,
            String groupType = null, String groupTopic = null, String externalName = null, String project = null)
            => client.UpdateMachineGroupAsync(new UpdateMachineGroupRequest(groupName, machineIdentifyType, machineList)
            {
                GroupType = groupType,
                GroupAttribute = groupTopic == null && externalName == null
                    ? null
                    : new MachineGroupAttributeInfo
                    {
                        GroupTopic = groupTopic,
                        ExternalName = externalName
                    },
                ProjectName = project
            });

        /// <summary>
        /// 列出 MachineGroup 信息。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">返回记录的起始位置，默认为 0。</param>
        /// <param name="offset">每页返回最大条目，默认 500（最大值）。</param>
        /// <param name="size">用于过滤的机器组名称（支持部分匹配）。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.ListMachineGroupAsync"/>
        public static Task<IResponse<ListMachineGroupResult>> ListMachineGroupAsync(this ILogServiceClient client,
            String groupName = null, Int32 offset = ListMachineGroupRequest.DefaultOffset, Int32 size = ListMachineGroupRequest.DefaultSize, String project = null)
            => client.ListMachineGroupAsync(new ListMachineGroupRequest
            {
                GroupName = groupName,
                Offset = offset,
                Size = size,
                ProjectName = project
            });

        /// <summary>
        /// 查看具体的 MachineGroup 信息。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">机器分组名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetMachineGroupAsync"/>
        public static Task<IResponse<GetMachineGroupResult>> GetMachineGroupAsync(this ILogServiceClient client,
            String groupName,
            String project = null)
            => client.GetMachineGroupAsync(new GetMachineGroupRequest(groupName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 将配置应用到机器组。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">机器分组名称。</param>
        /// <param name="configName">日志配置名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.ApplyConfigToMachineGroupAsync"/>
        public static Task<IResponse> ApplyConfigToMachineGroupAsync(this ILogServiceClient client,
            String groupName, String configName,
            String project = null)
            => client.ApplyConfigToMachineGroupAsync(new ApplyConfigToMachineGroupRequest(groupName, configName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 从机器组中删除配置。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">机器分组名称。</param>
        /// <param name="configName">日志配置名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.RemoveConfigFromMachineGroupAsync"/>
        public static Task<IResponse> RemoveConfigFromMachineGroupAsync(this ILogServiceClient client,
            String groupName, String configName,
            String project = null)
            => client.RemoveConfigFromMachineGroupAsync(new RemoveConfigFromMachineGroupRequest(groupName, configName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 获得 MachineGroup 下属于用户并与 Server 端连接的机器状态信息。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">用于过滤的机器组名称（支持部分匹配）。</param>
        /// <param name="offset">返回记录的起始位置，默认为 0。</param>
        /// <param name="size">每页返回最大条目，默认 500（最大值）。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.ListMachinesAsync"/>
        public static Task<IResponse<ListMachinesResult>> ListMachinesAsync(this ILogServiceClient client,
            String groupName,
            Int32 offset = ListMachinesRequest.DefaultOffset, Int32 size = ListMachinesRequest.DefaultSize, String project = null)
            => client.ListMachinesAsync(new ListMachinesRequest(groupName)
            {
                Offset = offset,
                Size = size,
                ProjectName = project
            });

        /// <summary>
        /// 获得 MachineGroup 上已经被应用的配置名称。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="groupName">机器分组名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetAppliedConfigsAsync"/>
        public static Task<IResponse<GetAppliedConfigsResult>> GetAppliedConfigsAsync(this ILogServiceClient client,
            String groupName,
            String project = null)
            => client.GetAppliedConfigsAsync(new GetAppliedConfigsRequest(groupName)
            {
                ProjectName = project
            });

        #endregion MachineGroup

        #region Config

        /// <summary>
        /// 在 Project 下创建日志配置。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="configName">日志配置名称， Project 下唯一。</param>
        /// <param name="inputType">输入类型，现在只支持 file。</param>
        /// <param name="inputDetail">输入详情。</param>
        /// <param name="outputType">输出类型，现在只支持 LogService。</param>
        /// <param name="outputDetail">输出详情。</param>
        /// <param name="logSample">Logtail 配置日志样例，最大支持 1000 字节。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateConfigAsync"/>
        public static Task<IResponse> CreateConfigAsync(this ILogServiceClient client,
            String configName, String inputType, ConfigInputDetailInfo inputDetail, String outputType, ConfigOutputDetailInfo outputDetail,
            String logSample = null, String project = null)
            => client.CreateConfigAsync(new CreateConfigRequest(configName, inputType, inputDetail, outputType, outputDetail)
            {
                LogSample = logSample,
                ProjectName = project
            });

        /// <summary>
        /// 在 Project 下创建日志配置。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="configName">日志配置名称， Project 下唯一。</param>
        /// <param name="logstoreName"></param>
        /// <param name="logType">日志类型，现在只支持 common_reg_log。</param>
        /// <param name="logPath">日志所在的父目录，例如 /var/logs/。</param>
        /// <param name="filePattern">日志文件的Pattern，例如 access*.log。</param>
        /// <param name="localStorage">是否打开本地缓存，在服务端之间链路断开的情况下，本地可以缓存 1GB 日志。</param>
        /// <param name="timeFormat">日志时间格式, 如 %Y/%m/%d %H:%M:%S。</param>
        /// <param name="logBeginRegex">日志首行特征（正则表达式），由于匹配多行日志组成一条 log 的情况。</param>
        /// <param name="regex">日志对提取正则表达式。</param>
        /// <param name="key">日志提取后所生成的 Key。</param>
        /// <param name="filterKey">用于过滤日志所用到的 key，只有 key 的值满足对应 filterRegex 列中设定的正则表达式日志才是符合要求的。</param>
        /// <param name="filterRegex">和每个 filterKey 对应的正则表达式， filterRegex 的长度和 filterKey 的长度必须相同。</param>
        /// <param name="topicFormat">Topic 生成方式，支持以下四种类型：
        ///   <list type="bullet">
        ///     <item><description>用于将日志文件路径的某部分作为 topic，如 /var/log/(.*).log。</description></item>
        ///     <item><description>none，表示 topic 为空。</description></item>
        ///     <item><description>default，表示将日志文件路径作为 topic。</description></item>
        ///     <item><description>group_topic，表示将应用该配置的机器组 topic 属性作为 topic。</description></item>
        ///   </list>
        /// </param>
        /// <param name="preserve">true 代表监控目录永不超时，false 代表监控目录 30 分钟超时，默认值为 true。</param>
        /// <param name="preserveDepth">当设置 preserve 为 false 时，指定监控不超时目录的深度，最大深度支持 3。</param>
        /// <param name="fileEncoding">支持两种类型：utf8、gbk。</param>
        /// <param name="logSample">Logtail 配置日志样例，最大支持 1000 字节。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateConfigAsync"/>
        public static Task<IResponse> CreateFileToLogServiceConfigAsync(this ILogServiceClient client,
            String configName, String logstoreName, String logType, String logPath, String filePattern, Boolean localStorage, String timeFormat, String logBeginRegex, String regex, IEnumerable<String> key, IEnumerable<String> filterKey, IEnumerable<String> filterRegex,
            String topicFormat = null, Boolean preserve = ConfigInputDetailInfo.DefaultPreserve, Int32 preserveDepth = default, String fileEncoding = null, String logSample = null, String project = null)
            => client.CreateConfigAsync(
                configName,
                "file",
                new ConfigInputDetailInfo(logType, logPath, filePattern, localStorage, timeFormat, logBeginRegex, regex, key, filterKey, filterRegex)
                {
                    TopicFormat = topicFormat,
                    Preserve = preserve,
                    PreserveDepth = preserveDepth,
                    FileEncoding = fileEncoding
                },
                "LogService",
                new ConfigOutputDetailInfo(logstoreName),
                logSample,
                project);

        /// <summary>
        /// 列出 Project 下所有配置信息，可以通过参数进行翻页。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="offset">返回记录的起始位置，默认为 0。</param>
        /// <param name="size">每页返回最大条目，默认 500（最大值）。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.ListConfigAsync"/>
        public static Task<IResponse<ListConfigResult>> ListConfigAsync(this ILogServiceClient client,
            Int32 offset = ListConfigRequest.DefaultOffset, Int32 size = ListConfigRequest.DefaultSize, String project = null)
            => client.ListConfigAsync(new ListConfigRequest
            {
                Offset = offset,
                Size = size,
                ProjectName = project
            });

        /// <summary>
        /// 列出 config 应用的机器列表。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="configName">日志配置名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetAppliedMachineGroupsAsync"/>
        public static Task<IResponse<GetAppliedMachineGroupsResult>> GetAppliedMachineGroupsAsync(this ILogServiceClient client,
            String configName,
            String project = null)
            => client.GetAppliedMachineGroupsAsync(new GetAppliedMachineGroupsRequest(configName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 获得一个配置的详细信息。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="configName">日志配置名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetConfigAsync"/>
        public static Task<IResponse<GetConfigResult>> GetConfigAsync(this ILogServiceClient client,
            String configName,
            String project = null)
            => client.GetConfigAsync(new GetConfigRequest(configName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 删除特定 config，如果 config 已被 应用到机器组，则 Logtail 配置也会被删除。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="configName">日志配置名称。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.DeleteConfigAsync"/>
        public static Task<IResponse> DeleteConfigAsync(this ILogServiceClient client,
            String configName,
            String project = null)
            => client.DeleteConfigAsync(new DeleteConfigRequest(configName)
            {
                ProjectName = project
            });

        /// <summary>
        /// 更新配置内容，如果配置被应用到机器组，对应机器也会同时更新。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="configName">日志配置名称， Project 下唯一。</param>
        /// <param name="inputType">输入类型，现在只支持 file。</param>
        /// <param name="inputDetail">输入详情。</param>
        /// <param name="outputType">输出类型，现在只支持 LogService。</param>
        /// <param name="outputDetail">输出详情。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.UpdateConfigAsync"/>
        public static Task<IResponse> UpdateConfigAsync(this ILogServiceClient client,
            String configName, String inputType, ConfigInputDetailInfo inputDetail, String outputType, ConfigOutputDetailInfo outputDetail,
            String project = null)
            => client.UpdateConfigAsync(new UpdateConfigRequest(configName, inputType, inputDetail, outputType, outputDetail)
            {
                ProjectName = project
            });

        #endregion Config

        #region Project

        /// <summary>
        /// 创建project。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="projectName">project的名称，全局唯一。</param>
        /// <param name="projectDesc">project描述。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.CreateProjectAsync"/>
        public static Task<IResponse> CreateProjectAsync(this ILogServiceClient client,
            String projectName, String projectDesc)
            => client.CreateProjectAsync(new CreateProjectRequest(projectName, projectDesc));

        /// <summary>
        /// 获取指定区域Project列表。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="projectName">用于过滤返回结果的project名称（支持部分匹配）。</param>
        /// <param name="offset">请求结果的起始位置，默认为0。</param>
        /// <param name="size">每次请求返回结果最大数量，默认500（最大值）。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.ListProjectAsync"/>
        public static Task<IResponse<ListProjectResult>> ListProjectAsync(this ILogServiceClient client,
            String projectName = "", Int32 offset = ListProjectRequest.DefaultOffset, Int32 size = ListProjectRequest.DefaultSize)
            => client.ListProjectAsync(new ListProjectRequest
            {
                ProjectName = projectName,
                Offset = offset,
                Size = size
            });

        /// <summary>
        /// 获取当前Project信息。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.GetProjectAsync"/>
        public static Task<IResponse<GetProjectResult>> GetProjectAsync(this ILogServiceClient client,
            String project = null)
            => client.GetProjectAsync(new GetProjectRequest
            {
                ProjectName = project
            });

        /// <summary>
        /// 删除当前Project。
        /// </summary>
        /// <param name="client">client实例。</param>
        /// <param name="project">项目名，此参数将覆盖 client 中默认设置。</param>
        /// <returns>异步响应结果。</returns>
        /// <seealso cref="ILogServiceClient.DeleteProjectAsync"/>
        public static Task<IResponse> DeleteProjectAsync(this ILogServiceClient client,
            String project = null)
            => client.DeleteProjectAsync(new DeleteProjectRequest
            {
                ProjectName = project
            });

        #endregion
    }
}
