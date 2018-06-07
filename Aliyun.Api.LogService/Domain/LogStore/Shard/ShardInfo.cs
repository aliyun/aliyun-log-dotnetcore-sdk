//
// ShardInfo.cs
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

namespace Aliyun.Api.LogService.Domain.LogStore.Shard
{
    public class ShardInfo
    {
        /// <summary>
        /// Shard ID，分区号。
        /// </summary>
        public Int32 ShardId { get; }

        /// <summary>
        /// 分区的状态。
        /// <list type="bullet">
        ///   <item><description>readwrite：可以读写</description></item>
        ///   <item><description>readonly：只读数据</description></item>
        /// </list>
        /// </summary>
        public ShardState Status { get; }

        /// <summary>
        /// 分区起始的Key值，分区范围中包含该Key值。
        /// </summary>
        public String InclusiveBeginKey { get; }

        /// <summary>
        /// 分区结束的Key值，分区范围中不包含该Key值。
        /// </summary>
        public String ExclusiveEndKey { get; }

        /// <summary>
        /// 分区创建时间。
        /// </summary>
        public Int64 CreateTime { get; }

        public ShardInfo(Int32 shardId, ShardState status, String inclusiveBeginKey, String exclusiveEndKey, Int64 createTime)
        {
            this.ShardId = shardId;
            this.Status = status;
            this.InclusiveBeginKey = inclusiveBeginKey;
            this.ExclusiveEndKey = exclusiveEndKey;
            this.CreateTime = createTime;
        }
    }
}
