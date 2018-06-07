//
// GetLogStoreResult.cs
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

namespace Aliyun.Api.LogService.Domain.LogStore
{
    public class GetLogStoreResult
    {
        ///<summary>
        /// logstore 的名称， 在该 project 下唯一。
        ///</summary>
        public String LogstoreName { get; }

        ///<summary>
        /// 日志数据生命周期（TTL），单位为天，最小为 1 天。
        ///</summary>
        public Int32 Ttl { get; }

        ///<summary>
        /// 日志数据 服务单元。
        ///</summary>
        public Int32 ShardCount { get; }

        ///<summary>
        /// 该资源服务端创建时间。
        ///</summary>
        public Int32 CreateTime { get; }

        ///<summary>
        /// 该资源服务端更新时间。
        ///</summary>
        public Int32 LastModifyTime { get; }

        public GetLogStoreResult(String logstoreName, Int32 ttl, Int32 shardCount, Int32 createTime, Int32 lastModifyTime)
        {
            this.LogstoreName = logstoreName;
            this.Ttl = ttl;
            this.ShardCount = shardCount;
            this.CreateTime = createTime;
            this.LastModifyTime = lastModifyTime;
        }
    }
}
