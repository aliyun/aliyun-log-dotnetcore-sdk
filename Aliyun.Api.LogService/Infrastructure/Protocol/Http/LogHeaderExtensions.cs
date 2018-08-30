//
// LogServiceClientBuilders.cs
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
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    public static class LogHeaderExtensions
    {
        private static T? ParseNullable<T>(this String source, Func<String, T> parse) where T : struct
        {
            if (source.IsEmpty())
            {
                return null;
            }

            return parse(source);
        }

        private static T ParseNotNull<T>(this String source, Func<String, T> parse)
        {
            return parse(source);
        }

        #region PullLogs

        /// <summary>
        /// 获取下一条数据的cursor。
        /// </summary>
        /// <param name="response"><c>PullLogs</c> 的响应消息。</param>
        /// <returns>下一条数据的cursor。</returns>
        public static String GetLogCursor(this IResponse<PullLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.Cursor);

        /// <summary>
        /// 获取当前返回数量。
        /// </summary>
        /// <param name="response"><c>PullLogs</c> 的响应消息。</param>
        /// <returns>当前返回数量。</returns>
        /// <exception cref="FormatException">Header的值不是整数形式。</exception>
        public static Int64 GetLogCount(this IResponse<PullLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.Count)
                .ParseNotNull(Int64.Parse);

        /// <summary>
        /// 获取报文压缩类型（可空）。
        /// </summary>
        /// <param name="response"><c>PullLogs</c> 的响应消息。</param>
        /// <returns>报文压缩类型（可空）。</returns>
        /// <exception cref="OverflowException">Header的值在当前SDK版本不支持。</exception>
        /// <seealso cref="CompressType"/>
        public static CompressType? GetLogCompressType(this IResponse<PullLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.CompressType)
                .ParseNullable(x => (CompressType) Enum.Parse(typeof(CompressType), x, true));

        /// <summary>
        /// 获取响应消息中 Body 的原始大小。
        /// </summary>
        /// <param name="response"><c>PullLogs</c> 的响应消息。</param>
        /// <returns>响应消息中 Body 的原始大小。</returns>
        /// <exception cref="FormatException">当Header的值不是整数形式。</exception>
        public static Int64 GetLogBodyRawSize(this IResponse<PullLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.BodyRawSize)
                .ParseNotNull(Int64.Parse);

        #endregion

        #region GetLogs

        /// <summary>
        /// 获取查询结果的状态。
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns>获取查询结果的状态。</returns>
        /// <exception cref="OverflowException">Header的值在当前SDK版本不支持。</exception>
        /// <seealso cref="LogProgressState"/>
        public static LogProgressState GetLogProgress(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.Progress)
                .ParseNotNull(x => (LogProgressState) Enum.Parse(typeof(LogProgressState), x, true));

        /// <summary>
        /// 获取当前返回数量。
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns>当前返回数量。</returns>
        /// <exception cref="FormatException">Header的值不是整数形式。</exception>
        public static Int64 GetLogCount(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.Count)
                .ParseNotNull(Int64.Parse);

        /// <summary>
        /// （TODO: 暂无文档）
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Header的值不是整数形式。</exception>
        public static Int64 GetLogProcessedRows(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.ProcessedRows)
                .ParseNotNull(Int64.Parse);

        /// <summary>
        /// （TODO: 暂无文档）
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Header的值不是整数形式。</exception>
        public static Int64 GetLogElapsedMillisecond(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.ElapsedMillisecond)
                .ParseNotNull(Int64.Parse);

        /// <summary>
        /// （TODO: 暂无文档）
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns></returns>
        public static LogQueryInfo GetLogQueryInfo(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.QueryInfo)
                .ParseNotNull(JsonConvert.DeserializeObject<LogQueryInfo>);

        /// <summary>
        /// （TODO: 暂无文档）
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Header的值不是JSON对象形式。</exception>
        public static dynamic GetLogQueryInfoAsDynamic(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.QueryInfo)
                .ParseNotNull(x =>
                {
                    if (x.IsEmpty())
                    {
                        return new JObject();
                    }

                    try
                    {
                        return JObject.Parse(x);
                    } catch (JsonReaderException e)
                    {
                        // Prevent underlying type expose explicitly.
                        throw new FormatException(e.Message, e);
                    }
                });

        /// <summary>
        /// （TODO: 暂无文档）
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns></returns>
        public static Boolean GetLogHasSql(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.HasSql)
                .ParseNotNull(Boolean.Parse);

        /// <summary>
        /// （TODO: 暂无文档）
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns></returns>
        public static String GetLogAggQuery(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.AggQuery);

        /// <summary>
        /// （TODO: 暂无文档）
        /// </summary>
        /// <param name="response"><c>GetLogs</c> 的响应消息。</param>
        /// <returns></returns>
        public static String GetLogWhereQuery(this IResponse<GetLogsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.WhereQuery);

        #endregion

        #region GetHistograms

        /// <summary>
        /// 获取当前返回数量。
        /// </summary>
        /// <param name="response"><c>GetLogHistograms</c> 的响应消息。</param>
        /// <returns>当前返回数量。</returns>
        /// <exception cref="FormatException">Header的值不是整数形式。</exception>
        public static Int64 GetLogCount(this IResponse<GetLogHistogramsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.Count)
                .ParseNotNull(Int64.Parse);

        /// <summary>
        /// 获取查询结果的状态。
        /// </summary>
        /// <param name="response"><c>GetLogHistograms</c> 的响应消息。</param>
        /// <returns>获取查询结果的状态。</returns>
        /// <exception cref="OverflowException">Header的值在当前SDK版本不支持。</exception>
        /// <seealso cref="LogProgressState"/>
        public static LogProgressState GetLogProgress(this IResponse<GetLogHistogramsResult> response)
            => response.Headers.GetValueOrDefault(LogHeaders.Progress)
                .ParseNotNull(x => (LogProgressState) Enum.Parse(typeof(LogProgressState), x, true));

        #endregion
    }
}
