//
// InvokeUsingTplExample.cs
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

namespace Aliyun.Api.LogService.Examples.Invocation
{
    public static class InvokeUsingTplExample
    {
        /// <summary>
        /// 在没有 async/await 语法（C# 5.0 以下）的情况下可以使用 TPL 的 `ContinueWith` 方法。
        /// </summary>
        public static Task<GetLogsResult> InvokeUsingTpl(ILogServiceClient client)
            => client.GetLogsAsync
                (
                    // 「必填参数」作为方法的普通必须参数
                    "example-logstore",
                    DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow,

                    // 「可选参数」作为方法的可选参数，可通过命名参数方式指定
                    offset: 1,
                    line: 10
                )
                .ContinueWith(task =>
                {
                    var response = task.Result; // 此处获取 `Result` 不会阻塞，因为在 `ContinueWith` 方法中保证了前置任务必定已完成。

                    var result = response
                        // 此方法会确保返回的响应失败时候抛出`LogServiceException`。
                        .EnsureSuccess()
                        // 此处获取Result是安全的。
                        .Result;

                    Console.WriteLine($"RequestId：{response.RequestId}");
                    Console.WriteLine($"日志总数：{result.Count}");
                    Console.WriteLine($"首条日志：{result.Logs.FirstOrDefault()}");

                    return result;
                });
    }
}
