//
// ListMachinesResult.cs
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

namespace Aliyun.Api.LogService.Domain.MachineGroup
{
    public class ListMachinesResult
    {
        /// <summary>
        /// 返回的 machinegroup 数目。
        /// </summary>
        public Int32 Count { get; }

        /// <summary>
        /// 返回 machinegroup 总数。
        /// </summary>
        public Int32 Total { get; }

        /// <summary>
        /// 返回的 machinegroup 名称列表。
        /// </summary>
        public IList<MachineInfo> Machines { get; }

        public ListMachinesResult(Int32 count, Int32 total, IList<MachineInfo> machines)
        {
            this.Count = count;
            this.Total = total;
            this.Machines = machines;
        }

        public class MachineInfo
        {
            /// <summary>
            /// 机器的 IP。
            /// </summary>
            public String Ip { get; }

            /// <summary>
            /// 机器 DMI UUID。
            /// </summary>
            public String MachineUniqueId { get; }

            /// <summary>
            /// 机器的用户自定义标识。
            /// </summary>
            public String UserDefinedId { get; }

            /// <summary>
            /// 机器最后的心跳时间。
            /// </summary>
            public String LastHeartbeatTime { get; }

            public MachineInfo(String ip, String machineUniqueId, String userDefinedId, String lastHeartbeatTime)
            {
                this.Ip = ip;
                this.MachineUniqueId = machineUniqueId;
                this.UserDefinedId = userDefinedId;
                this.LastHeartbeatTime = lastHeartbeatTime;
            }
        }
    }
}
