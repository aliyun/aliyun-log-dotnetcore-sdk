//
// RetryShipperRequest.cs
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

namespace Aliyun.Api.LogService.Domain.LogStore.Shipper
{
    public class RetryShipperRequest : ProjectScopedRequest
    {
        /// <summary>
        /// 日志库名称，同一 Project 下唯一。
        /// </summary>
        public String LogstoreName { get; }

        /// <summary>
        /// 日志投递规则名称，同一 Logstore 下唯一。
        /// </summary>
        public String ShipperName { get; }

        /// <summary>
        /// 需要重试的任务ID。
        /// </summary>
        public IEnumerable<String> TaskIds { get; }

        public RetryShipperRequest(String logstoreName, String shipperName, IEnumerable<String> taskIds)
        {
            this.LogstoreName = logstoreName;
            this.ShipperName = shipperName;
            this.TaskIds = taskIds;
        }
    }
}
