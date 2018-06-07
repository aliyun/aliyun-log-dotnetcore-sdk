//
// LogHistogramInfo.cs
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

namespace Aliyun.Api.LogService.Domain.Log
{
    public class LogHistogramInfo
    {
        /// <summary>
        /// 子时间区间的开始时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。
        /// </summary>
        public Int32 From { get; }

        /// <summary>
        /// 子时间区间的结束时间点（精度为秒，从 1970-1-1 00:00:00 UTC 计算起的秒数）。
        /// </summary>
        public Int32 To { get; }

        /// <summary>
        /// 当前查询结果在该子时间区间内命中的日志条数。
        /// </summary>
        public Int32 Count { get; }

        /// <summary>
        /// 当前查询结果在该子时间区间内的结果是否完整，可以有 Incomplete 和 Complete 两个选值。
        /// </summary>
        public LogProgressState Progress { get; }

        public LogHistogramInfo(Int32 from, Int32 to, Int32 count, LogProgressState progress)
        {
            this.From = from;
            this.To = to;
            this.Count = count;
            this.Progress = progress;
        }
    }
}
