//
// InvokeAsyncExample.cs
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
using Aliyun.Api.LogService.Domain;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Infrastructure.Protocol;

namespace Aliyun.Api.LogService.Examples.Invocation
{
    public static class InvokeAsyncExample
    {
        /// <summary>
        /// 执行请求方法。
        /// </summary>
        public static async Task<GetLogsResult> Invoke(ILogServiceClient client)
        {
            var response = await client.GetLogsAsync
            (
                // 「必填参数」会在 Request 构造器中列出，并且不可set；
                new GetLogsRequest("example-logstore", (Int32) DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(), (Int32) DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    // 「可选参数」不会在 Request 构造器中列出，可通过setter设置。
                    Offset = 1,
                    Line = 100,
                }
            );

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

        /// <summary>
        /// 在调用时可使用扩展方法，扩展方法会将简单的请求对象的属性展开到方法入参中。
        /// 使用扩展方法 `using Aliyun.Api.Log;` 即可。
        /// </summary>
        public static async Task<GetLogsResult> InvokeUsingExtension(ILogServiceClient client)
        {
            var response = await client.GetLogsAsync
            (
                // 「必填参数」作为方法的普通必须参数
                "example-logstore",
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow,

                // 「可选参数」作为方法的可选参数，可通过命名参数方式指定
                offset: 1,
                line: 10
            );

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

        /// <summary>
        /// 处理服务器返回（包含在Response中）的错误。
        /// </summary>
        public static async Task<GetLogsResult> InvokeWithErrorHandling(ILogServiceClient client)
        {
            var response = await client.GetLogsAsync
            (
                // 「必填参数」作为方法的普通必须参数
                "example-logstore",
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow,

                // 「可选参数」作为方法的可选参数，可通过命名参数方式指定
                offset: 1,
                line: 10
            );

            GetLogsResult result;
            // 尝试处理可处理的错误。
            if (!response.IsSuccess)
            {
                // 错误码
                var errorCode = response.Error.ErrorCode;
                // 错误消息
                var errorMessage = response.Error.ErrorMessage;

                Console.WriteLine($"RequestId：{response.RequestId}");
                Console.WriteLine($"错误码：{errorCode}");
                Console.WriteLine($"错误信息：{errorMessage}");
                
                // `ErrorCode`类可支持与自身实例或字符串进行对比。
                if (errorCode == ErrorCode.SignatureNotMatch /* SDK中预定义的错误码 */)
                {
                    // 在这里处理业务可处理的错误。。。。。。
                    Console.WriteLine("Signature not match, {0}.", errorMessage);
                } else if (errorCode == "ParameterInvalid" /* 业务相关特殊的SDK中未定义的错误码 */)
                {
                    // 在这里处理业务可处理的错误。。。。。。
                    Console.WriteLine("Parameter invalid, {0}.", errorMessage);
                }

                // 任何处理不到的错误请务必抛出异常中断原流程，避免外部获取到 null 的结果！
                throw new Exception("这里可以是系统的业务异常。" + response.Error /* 最好带上服务返回的错误信息以便调试 */);
            } else
            {
                // 此处获取Result是安全的。
                result = response.Result;
            }

            Console.WriteLine($"RequestId：{response.RequestId}");
            Console.WriteLine($"日志总数：{result.Count}");
            Console.WriteLine($"首条日志：{result.Logs.FirstOrDefault()}");

            return result;
        }

        /// <summary>
        /// 处理以异常形式抛出的错误。
        /// </summary>
        public static async Task<GetLogsResult> InvokeWithExceptionHandling(ILogServiceClient client)
        {
            try
            {
                return await Invoke(client);
            } catch (LogServiceException e)
            {
                // 错误码
                var errorCode = e.ErrorCode;
                // 错误消息
                var errorMessage = e.ErrorMessage;

                Console.WriteLine($"RequestId：{e.RequestId}");
                Console.WriteLine($"错误码：{errorCode}");
                Console.WriteLine($"错误信息：{errorMessage}");

                // `ErrorCode`类可支持与自身实例或字符串进行对比。
                if (errorCode == ErrorCode.SignatureNotMatch /* SDK中预定义的错误码 */)
                {
                    // 在这里处理业务可处理的错误。。。。。。
                    Console.WriteLine("Signature not match, {0}.", errorMessage);
                } else if (errorCode == "ParameterInvalid" /* 业务相关特殊的SDK中未定义的错误码 */)
                {
                    // 在这里处理业务可处理的错误。。。。。。
                    Console.WriteLine("Parameter invalid, {0}.", errorMessage);
                }

                // 任何处理不到的错误请务必抛出异常中断原流程，避免外部获取到 null 的结果！
                throw new Exception("这里可以是系统的业务异常。", e /* 在自定义的异常中最好带上服务返回的异常以便调试 */);
            }
        }
    }
}
