//
// GetConfigResult.cs
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

namespace Aliyun.Api.LogService.Domain.Config
{
    public class GetConfigResult
    {
        /// <summary>
        /// 日志配置名称， Project 下唯一。
        /// </summary>
        public String ConfigName { get; set; }

        /// <summary>
        /// 输入类型，现在只支持 file。
        /// </summary>
        public String InputType { get; set; }

        /// <summary>
        /// 输入详情。
        /// </summary>
        public ConfigInputDetailInfo InputDetail { get; set; }

        /// <summary>
        /// 输出类型，现在只支持 LogService。
        /// </summary>
        public String OutputType { get; set; }

        /// <summary>
        /// 输出详情。
        /// </summary>
        public ConfigOutputDetailExtInfo OutputDetail { get; set; }

        /// <summary>
        /// 配置创建时间。
        /// </summary>
        public Int32 CreateTime { get; set; }

        /// <summary>
        /// 该资源服务端更新时间。
        /// </summary>
        public Int32 LastModifyTime { get; set; }

        public GetConfigResult(String configName, String inputType, ConfigInputDetailInfo inputDetail, String outputType, ConfigOutputDetailExtInfo outputDetail, Int32 createTime, Int32 lastModifyTime)
        {
            this.ConfigName = configName;
            this.InputType = inputType;
            this.InputDetail = inputDetail;
            this.OutputType = outputType;
            this.OutputDetail = outputDetail;
            this.CreateTime = createTime;
            this.LastModifyTime = lastModifyTime;
        }

        public GetConfigResult()
        {

        }
        
        public class ConfigOutputDetailExtInfo : ConfigOutputDetailInfo
        {
            /// <summary>
            /// Project 所在的访问地址。
            /// </summary>
            public String Endpoint { get; set; }
            
            public ConfigOutputDetailExtInfo(String logstoreName, String endpoint)
                : base(logstoreName)
            {
                this.Endpoint = endpoint;
            }

            public ConfigOutputDetailExtInfo()
            {

            }
        }
    }
}
