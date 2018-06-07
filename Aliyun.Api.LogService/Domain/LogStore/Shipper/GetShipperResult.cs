//
// GetShipperResult.cs
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

namespace Aliyun.Api.LogService.Domain.LogStore.Shipper
{
    public class GetShipperResult
    {
        /// <summary>
        /// 返回的任务个数。
        /// </summary>
        public Int32 Count { get; }

        /// <summary>
        /// 指定范围内任务总数。
        /// </summary>
        public Int32 Total { get; }

        /// <summary>
        /// 指定范围内任务汇总状态，具体请参考下表。
        /// </summary>
        public StatisticsResult Statistics { get; }

        /// <summary>
        /// 指定范围内投递任务具体详情，具体请参考下表。   
        /// </summary>
        public IList<TasksResult> Tasks { get; }

        public GetShipperResult(Int32 count, Int32 total, StatisticsResult statistics, IList<TasksResult> tasks)
        {
            this.Count = count;
            this.Total = total;
            this.Statistics = statistics;
            this.Tasks = tasks;
        }

        public class StatisticsResult
        {
            /// <summary>
            /// 指定范围内状态为 running 的任务个数。
            /// </summary>
            public Int32 Running { get; }

            /// <summary>
            /// 指定范围内状态为 success 的任务个数。
            /// </summary>
            public Int32 Success { get; }

            /// <summary>
            /// 指定范围内状态为 fail 的任务个数。
            /// </summary>
            public Int32 Fail { get; }

            public StatisticsResult(Int32 running, Int32 success, Int32 fail)
            {
                this.Running = running;
                this.Success = success;
                this.Fail = fail;
            }
        }

        public class TasksResult
        {
            /// <summary>
            /// 具体投递任务的任务唯一 ID。
            /// </summary>
            public String Id { get; }

            /// <summary>
            /// 投递任务的具体状态，可能为 running/success/fail 中的一种。
            /// </summary>
            public String TaskStatus { get; }

            /// <summary>
            /// 投递任务失败时的具体错误信息。
            /// </summary>
            public String TaskMessage { get; }

            /// <summary>
            /// 投递任务创建时间。
            /// </summary>
            public Int32 TaskCreateTime { get; }

            /// <summary>
            /// 投递任务中的最近一条日志到达服务端时间（非日志时间，是服务端接收时间）。
            /// </summary>
            public Int32 TaskLastDataReceiveTime { get; }

            /// <summary>
            /// 投递任务结束时间。
            /// </summary>
            public Int32 TaskFinishTime { get; }

            public TasksResult(String id, String taskStatus, String taskMessage, Int32 taskCreateTime, Int32 taskLastDataReceiveTime, Int32 taskFinishTime)
            {
                this.Id = id;
                this.TaskStatus = taskStatus;
                this.TaskMessage = taskMessage;
                this.TaskCreateTime = taskCreateTime;
                this.TaskLastDataReceiveTime = taskLastDataReceiveTime;
                this.TaskFinishTime = taskFinishTime;
            }
        }
    }
}
