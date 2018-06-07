//
// ConfigInputDetailInfo.cs
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
using System.Linq;

namespace Aliyun.Api.LogService.Domain.Config
{
    public class ConfigInputDetailInfo
    {
        public const Boolean DefaultPreserve = true;
        
        /// <summary>
        /// 日志类型，现在只支持 common_reg_log。
        /// </summary>
        public String LogType { get; }

        /// <summary>
        /// 日志所在的父目录，例如 /var/logs/。
        /// </summary>
        public String LogPath { get; }

        /// <summary>
        /// 日志文件的Pattern，例如 access*.log。
        /// </summary>
        public String FilePattern { get; }

        /// <summary>
        /// 是否打开本地缓存，在服务端之间链路断开的情况下，本地可以缓存 1GB 日志。
        /// </summary>
        public Boolean LocalStorage { get; }

        /// <summary>
        /// 日志时间格式, 如 %Y/%m/%d %H:%M:%S。
        /// </summary>
        public String TimeFormat { get; }

        /// <summary>
        /// 日志首行特征（正则表达式），由于匹配多行日志组成一条 log 的情况。
        /// </summary>
        public String LogBeginRegex { get; }

        /// <summary>
        /// 日志对提取正则表达式。
        /// </summary>
        public String Regex { get; }

        /// <summary>
        /// 日志提取后所生成的 Key。
        /// </summary>
        public IList<String> Key { get; }

        /// <summary>
        /// 用于过滤日志所用到的 key，只有 key 的值满足对应 filterRegex 列中设定的正则表达式日志才是符合要求的。
        /// </summary>
        public IList<String> FilterKey { get; }

        /// <summary>
        /// 和每个 filterKey 对应的正则表达式， filterRegex 的长度和 filterKey 的长度必须相同。
        /// </summary>
        public IList<String> FilterRegex { get; }

        /// <summary>
        /// Topic 生成方式，支持以下四种类型：
        /// <list type="bullet">
        ///   <item><description>用于将日志文件路径的某部分作为 topic，如 /var/log/(.*).log。</description></item>
        ///   <item><description>none，表示 topic 为空。</description></item>
        ///   <item><description>default，表示将日志文件路径作为 topic。</description></item>
        ///   <item><description>group_topic，表示将应用该配置的机器组 topic 属性作为 topic。</description></item>
        /// </list>
        /// </summary>
        public String TopicFormat { get; set; }

        /// <summary>
        /// true 代表监控目录永不超时，false 代表监控目录 30 分钟超时，默认值为 true。
        /// </summary>
        public Boolean Preserve { get; set; } = DefaultPreserve;

        /// <summary>
        /// 当设置 preserve 为 false 时，指定监控不超时目录的深度，最大深度支持 3。
        /// </summary>
        public Int32 PreserveDepth { get; set; }

        /// <summary>
        /// 支持两种类型：utf8、gbk。
        /// </summary>
        public String FileEncoding { get; set; }

        public ConfigInputDetailInfo(String logType, String logPath, String filePattern, Boolean localStorage, String timeFormat, String logBeginRegex, String regex, IEnumerable<String> key, IEnumerable<String> filterKey, IEnumerable<String> filterRegex)
        {
            this.LogType = logType;
            this.LogPath = logPath;
            this.FilePattern = filePattern;
            this.LocalStorage = localStorage;
            this.TimeFormat = timeFormat;
            this.LogBeginRegex = logBeginRegex;
            this.Regex = regex;
            this.Key = key?.ToList();
            this.FilterKey = filterKey?.ToList();
            this.FilterRegex = filterRegex?.ToList();
        }
    }
}
