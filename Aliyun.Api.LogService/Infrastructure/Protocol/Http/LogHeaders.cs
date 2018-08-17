//
// LogHeaders.cs
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
using Aliyun.Api.LogService.Domain.Log;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    /// <summary>
    /// 日志服务扩展Header。
    /// </summary>
    public static class LogHeaders
    {
        /// <summary>
        /// API 的版本号。
        /// </summary>
        public static readonly String ApiVersion = $"x-log-{nameof(ApiVersion).ToLower()}";

        /// <summary>
        /// 请求的 Body 原始大小。
        /// 当无 Body 时，该字段为 0；
        /// 当 Body 是压缩数据，则为压缩前的原始数据大小。
        /// 该域取值范围为 0 ~ 3 * 1024 * 1024。
        /// 该字段为非必选字段，只在压缩时需要。
        /// </summary>
        public static readonly String BodyRawSize = $"x-log-{nameof(BodyRawSize).ToLower()}";

        /// <summary>
        /// API 请求中 Body 部分使用的压缩方式。
        /// 如果不压缩可以不提供该请求头。
        /// </summary>
        /// <seealso cref="CompressType"/>
        public static readonly String CompressType = $"x-log-{nameof(CompressType).ToLower()}";

        /// <summary>
        /// 当前发送时刻的时间，格式和 Date 头一致。
        /// 该请求头为可选项。如果请求中包含该公共请求头，它的值会取代 Date 标准头的值用于服务端请求验证（该字段不参与签名）。
        /// 无论是否有 x-log-date 头，HTTP 标准 Date 头都必须提供。
        /// </summary>
        public static readonly String LogDate = $"x-log-{nameof(LogDate).ToLower()}";

        /// <summary>
        /// 签名计算方式。
        /// </summary>
        /// <seealso cref="SignatureType"/>
        public static readonly String SignatureMethod = $"x-log-{nameof(SignatureMethod).ToLower()}";

        /// <summary>
        /// 使用 STS 临时身份发送数据。当使用 STS 临时身份时必填，其他情况不要填写。
        /// </summary>
        public static readonly String SecurityToken = "x-acs-security-token";

        /// <summary>
        /// 服务端产生的标示该请求的唯一 ID。该响应头与具体应用无关，主要用于跟踪和调查问题。
        /// 如果用户希望调查出现问题的 API 请求，可以向 Log Service 团队提供该 ID。
        /// </summary>
        public static readonly String RequestId = $"x-log-{nameof(RequestId).ToLower()}";

        /// <summary>
        /// 日志上传的hash key，用来判断上传日志应该落在哪个 shard 中。
        /// </summary>
        public static readonly String HashKey = $"x-log-{nameof(HashKey).ToLower()}";

        /// <summary>
        /// 当前返回数量。
        /// </summary>
        public static readonly String Count = $"x-log-{nameof(Count).ToLower()}";

        /// <summary>
        /// 当前读取数据下一条 cursor。
        /// </summary>
        public static readonly String Cursor = $"x-log-{nameof(Cursor).ToLower()}";

        /// <summary>
        /// 查询结果的状态，表示本次是否完整。
        /// </summary>
        /// <seealso cref="LogProgressState"/>
        public static readonly String Progress = "x-log-progress";

        public static readonly String AggQuery = "x-log-agg-query";
        public static readonly String ElapsedMillisecond = "x-log-elapsed-millisecond";
        public static readonly String HasSql = "x-log-has-sql";
        public static readonly String ProcessedRows = "x-log-processed-rows";
        public static readonly String QueryInfo = "x-log-query-info";
        public static readonly String WhereQuery = "x-log-where-query";
    }
}
