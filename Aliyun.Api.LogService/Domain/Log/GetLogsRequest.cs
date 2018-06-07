//
// GetLogsRequest.cs
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
using Aliyun.Api.LogService.Domain.Project;

namespace Aliyun.Api.LogService.Domain.Log
{
    public class GetLogsRequest : ProjectScopedRequest
    {
        public const Int32 DefaultLine = 100;
        public const Int32 DefaultOffset = 0;
        public const Boolean DefaultReverse = false;

        /// <summary>
        /// 需要查询日志的 Logstore 名称。
        /// </summary>
        public String Logstorename { get; }

        /// <summary>
        /// 查询开始时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。
        /// </summary>
        public Int32 From { get; }

        /// <summary>
        /// 查询结束时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。
        /// </summary>
        public Int32 To { get; }

        /// <summary>
        /// 查询日志主题。
        /// </summary>
        public String Topic { get; set; }

        /// <summary>
        /// 查询表达式。关于查询表达式的详细语法，请参考 查询语法。
        /// </summary>
        public String Query { get; set; }

        /// <summary>
        /// 请求返回的最大日志条数。取值范围为 0~100，默认值为 100。
        /// </summary>
        public Int32 Line { get; set; }

        /// <summary>
        /// 请求返回日志的起始点。取值范围为 0 或正整数，默认值为 0。
        /// </summary>
        public Int32 Offset { get; set; }

        /// <summary>
        /// 是否按日志时间戳逆序返回日志。true 表示逆序，false 表示顺序，默认值为 false。
        /// </summary>
        public Boolean Reverse { get; set; }

        public GetLogsRequest(String logstorename, Int32 from, Int32 to)
        {
            this.Logstorename = logstorename;
            this.From = from;
            this.To = to;
        }
    }
}
