//
// InvokeSyncExample.cs
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
using Aliyun.Api.LogService.Domain.Log;

namespace Aliyun.Api.LogService.Examples.Invocation
{
    public static class InvokeSyncExample
    {
        /// <summary>
        /// 使用同步阻塞方式获取结果。
        /// </summary>
        public static GetLogsResult InvokeSynchronously(ILogServiceClient client)
        {
            var asyncTask = client.GetLogsAsync
            (
                // 「必填参数」作为方法的普通必须参数
                "example-logstore",
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow,

                // 「可选参数」作为方法的可选参数，可通过命名参数方式指定
                offset: 1,
                line: 10
            );

#if ASPNET || WINFORM

            // NOTE:
            // 需要注意的是，在 WinForm/ASP.NET 环境中，由于设置了 SynchronizationContext ，
            // 所有异步后续操作（Continuation）都会被回传到调用线程上执行，此时不能直接阻塞请求，否则会造成当前线程无限等待。
            // See: http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
            
            // 在上述环境中，需要使用同步调用，必须在另一线程中等待异步任务的结果。
            var waitTask = Task.Run(() => asyncTask.Result);
            var response = waitTask.Result;

#else

            // 在普通控制台的环境下同步等待结果直接调用即可。
            var response = asyncTask.Result;

#endif

            var result = response
                // 此方法会确保返回的响应失败时候抛出`LogServiceException`。
                .EnsureSuccess()
                // 此处获取Result是安全的。
                .Result;

            Console.WriteLine($"RequestId：{response.RequestId}");
            Console.WriteLine($"日志总数：{result.Count}");
            Console.WriteLine($"首条日志：{result.Logs.FirstOrDefault()}");

            return result;
        }
    }
}
