//
// IResponse.cs
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
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain;

namespace Aliyun.Api.LogService.Infrastructure.Protocol
{
    /// <summary>
    /// 服务响应包装对象，包含未反序列化的原始数据，可通过 <c>ReadXxxAsync()</c> 方法读取原始报文。
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// 当前响应是否为一个成功的响应。
        /// </summary>
        Boolean IsSuccess { get; }
        
        /// <summary>
        /// 服务端产生的标示该请求的唯一 ID。
        /// 该响应头与具体应用无关，主要用于跟踪和调查问题。
        /// 如果用户希望调查出现问题的 API 请求，可以向 Log Service 团队提供该 ID。
        /// </summary>
        String RequestId { get; }

        /// <summary>
        /// 当前响应的元数据。
        /// </summary>
        IDictionary<String, String> Headers { get; }

        /// <summary>
        /// 当前响应包含的错误信息，在<see cref="IsSuccess"/>为<c>false</c>时存在。
        /// </summary>
        Error Error { get; }

        /// <summary>
        /// 确保当前响应是成功的，否则将抛出包含错误码及错误消息的 <see cref="LogServiceException"/> 。
        /// </summary>
        /// <returns>当前响应实例。</returns>
        /// <exception cref="LogServiceException">当前响应的 <see cref="IsSuccess"/> 不为<c>true</c></exception>
        IResponse EnsureSuccess();

        /// <summary>
        /// 读取原始数据并反序列化为 <typeparamref name="TResult"/> 。
        /// 反序列化过程会根据<see cref="ContentType"/>确定原始数据类型。
        /// </summary>
        /// <typeparam name="TResult">反序列化结果的类型。</typeparam>
        /// <returns>反序列化对象异步结果。</returns>
        Task<TResult> ReadAsAsync<TResult>();

        /// <summary>
        /// 读取原始数据并且作为字节流的形式返回。
        /// </summary>
        /// <returns>异步字节数据流。</returns>
        Task<Stream> ReadAsByteStreamAsync();

        /// <summary>
        /// 读取原始数据并且作为字节数组的形式返回。
        /// </summary>
        /// <returns>异步字节数组。</returns>
        Task<Byte[]> ReadAsByteArrayAsync();
    }

    /// <summary>
    /// 服务响应包装对象，此类型包含一个已反序列化为 <typeparamref name="TResult" /> 的 <see cref="Result">结果对象</see>。
    /// </summary>
    /// <typeparam name="TResult">响应包含结果的类型。</typeparam>
    public interface IResponse<out TResult> : IResponse
        where TResult : class
    {
        /// <summary>
        /// 已反序列化的结果对象，在<see cref="IResponse.IsSuccess"/>为<c>true</c>时存在。
        /// </summary>
        TResult Result { get; }
        
        /// <summary>
        /// 确保当前响应是成功的，否则将抛出包含错误码及错误消息的 <see cref="LogServiceException"/> 。
        /// </summary>
        /// <returns>当前响应实例。</returns>
        /// <exception cref="LogServiceException">当前响应的 <see cref="IResponse.IsSuccess"/> 不为<c>true</c></exception>
        new IResponse<TResult> EnsureSuccess();
    }
}
