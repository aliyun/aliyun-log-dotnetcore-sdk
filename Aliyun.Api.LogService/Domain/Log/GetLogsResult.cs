//
// GetLogsResult.cs
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

namespace Aliyun.Api.LogService.Domain.Log
{
    public class GetLogsResult
    {
        /// <summary>
        /// 查询结果的状态。可以有 Incomplete 和 Complete 两个选值，表示本次是否完整。
        /// </summary>
        public LogProgressState Progress { get; set; }

        /// <summary>
        /// 当前查询结果返回的日志总数（非总数）。
        /// </summary>
        public Int32 Count { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int32 ProcessedRows { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Int32 ElapsedMillisecond { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean HasSql { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String AggQuery { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String WhereQuery { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LogQueryInfo QueryInfo { get; set; }
        
        /// <summary>
        /// 日志内容。
        /// </summary>
        public IList<IDictionary<String, String>> Logs { get; }

        public GetLogsResult(IList<IDictionary<String, String>> logs)
        {
            this.Logs = logs;
        }
    }
}
