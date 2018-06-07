//
// GetCursorRequest.cs
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

namespace Aliyun.Api.LogService.Domain.LogStore.Shard
{
    public class GetCursorRequest : ProjectScopedRequest
    {
        /// <summary>
        /// 日志库名称
        /// </summary>
        public String LogstoreName { get; }

        /// <summary>
        /// Shard ID
        /// </summary>
        public Int32 ShardId { get; }

        /// <summary>
        /// 时间点（UNIX下秒数），或 begin，end
        /// </summary>
        public String From { get; }

        public GetCursorRequest(String logstoreName, Int32 shardId, String from)
        {
            this.LogstoreName = logstoreName;
            this.ShardId = shardId;
            this.From = from;
        }
    }
}
