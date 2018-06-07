//
// CreateIndexRequest.cs
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
using Aliyun.Api.LogService.Domain.Project;
using Newtonsoft.Json;

namespace Aliyun.Api.LogService.Domain.LogStore.Index
{
    public class CreateIndexRequest : ProjectScopedRequest
    {
        /// <summary>
        /// Logstore 的名称，在 Project 下必须唯一。
        /// </summary>
        [JsonIgnore]
        public String LogstoreName { get; }

        /// <summary>
        /// 全文索引，对于日志中value的索引属性，全文索引和字段查询必须至少配置一类。
        /// </summary>
        public IndexLineInfo Line { get; }

        /// <summary>
        /// 字段查询，对于具体字段的value索引属性，全文索引和字段查询必须至少配置一类。
        /// </summary>
        public IDictionary<String, IndexKeyInfo> Keys { get; }

        public CreateIndexRequest(String logstoreName, IndexLineInfo line) : this(logstoreName, line, null)
        {
            // Delegate constructor.
        }

        public CreateIndexRequest(String logstoreName, IDictionary<String, IndexKeyInfo> keys) : this(logstoreName, null, keys)
        {
            // Delegate constructor.
        }

        public CreateIndexRequest(String logstoreName, IndexLineInfo line, IDictionary<String, IndexKeyInfo> keys)
        {
            this.Line = line;
            this.Keys = keys;
            this.LogstoreName = logstoreName;
        }
    }
}
