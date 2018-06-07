//
// PostLogStoreLogsExample.cs
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
using System.Linq;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain.Log;

namespace Aliyun.Api.LogService.Examples.ApiUsage
{
    public static class PostLogStoreLogsExample
    {
        private static readonly String LogStoreName = "example-logstore";

        public static async Task PostLogStoreLogs(ILogServiceClient client)
        {
            // 原始日志
            var rawLogs = new[]
            {
                "2018-05-04 12:34:56 INFO id=1 status=foo",
                "2018-05-04 12:34:57 INFO id=2 status=bar",
                "2018-05-04 12:34:58 INFO id=1 status=foo",
                "2018-05-04 12:34:59 WARN id=1 status=foo",
            };

            // 解释 LogInfo
            var parsedLogs = rawLogs
                .Select(x =>
                {
                    var components = x.Split(' ');

                    var date = components[0];
                    var time = components[1];
                    var level = components[2];
                    var id = components[3].Split('=');
                    var status = components[4].Split('=');

                    var logInfo = new LogInfo
                    {
                        Contents =
                        {
                            {"level", level},
                            {id[0], id[1]},
                            {status[0], status[1]},
                        },
                        Time = DateTimeOffset.ParseExact($"{date} {time}", "yyyy-MM-dd HH:mm:ss", null)
                    };

                    return logInfo;
                })
                .ToList();

            var logGroupInfo = new LogGroupInfo
            {
                Topic = "example",
                LogTags =
                {
                    {"example", "true"},
                },
                Logs = parsedLogs
            };

            var response = await client.PostLogStoreLogsAsync(LogStoreName, logGroupInfo);

            // 此接口没有返回结果，确保返回结果成功即可。
            response.EnsureSuccess();
        }
    }
}
