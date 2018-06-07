//
// IResponseResolver.cs
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
using System.Threading.Tasks;

namespace Aliyun.Api.LogService.Infrastructure.Protocol
{
    /// <summary>
    /// 响应消息解释器。
    /// </summary>
    public interface IResponseResolver
    {
        /// <summary>
        /// 使用 <typeparamref name="TResult"/> 作为目标对象类型。
        /// </summary>
        /// <typeparam name="TResult">目标对象类型</typeparam>
        /// <returns>带有结果对象类型的响应消息解释器。</returns>
        IResponseResolver<TResult> With<TResult>()
            where TResult : class;

        /// <summary>
        /// 设置解压缩原始数据的处理器，此操作会使默认解压缩处理器失效。
        /// </summary>
        /// <param name="decompressor">解压缩处理器。</param>
        /// <returns>当前解释器。</returns>
        IResponseResolver Decompress(Func<Byte[], Byte[]> decompressor);

        /// <summary>
        /// 设置反序列化原始数据的处理器，此操作会使默认反序列化处理器失效。
        /// </summary>
        /// <param name="deserializer">反序列化处理器。</param>
        /// <typeparam name="TResult">结果对象类型。</typeparam>
        /// <returns>带有结果对象类型的响应消息解释器。</returns>
        IResponseResolver<TResult> Deserialize<TResult>(Func<Byte[], TResult> deserializer)
            where TResult : class;

        /// <summary>
        /// 解释响应消息。
        /// </summary>
        /// <returns>异步解释结果。</returns>
        Task<IResponse> ResolveAsync();
        
        /// <summary>
        /// 解释响应消息，并反序列化为 <typeparamref name="TResult"/> 。
        /// </summary>
        /// <typeparam name="TResult">结果对象类型。</typeparam>
        /// <returns>异步解释结果。</returns>
        Task<IResponse<TResult>> ResolveAsync<TResult>()
            where TResult : class;
    }


    /// <summary>
    /// 带有结果对象类型的响应消息解释器。
    /// </summary>
    /// <typeparam name="TResult">结果对象类型。</typeparam>
    public interface IResponseResolver<TResult>
        where TResult : class
    {
        /// <summary>
        /// 设置解压缩原始数据的处理器，此操作会使默认解压缩处理器失效。
        /// </summary>
        /// <param name="decompressor">解压缩处理器。</param>
        /// <returns>当前解释器。</returns>
        IResponseResolver<TResult> Decompress(Func<Byte[], Byte[]> decompressor);

        /// <summary>
        /// 设置反序列化原始数据的处理器，此操作会使默认反序列化处理器失效。
        /// </summary>
        /// <param name="deserializer">反序列化处理器。</param>
        /// <returns>当前解释器。</returns>
        IResponseResolver<TResult> Deserialize(Func<Byte[], TResult> deserializer);

        /// <summary>
        /// 解释响应消息，并反序列化为 <typeparamref name="TResult"/> 。
        /// </summary>
        /// <returns>异步解释结果。</returns>
        Task<IResponse<TResult>> ResolveAsync();

        /// <summary>
        /// 解释响应消息，反序列化为 <typeparamref name="TResult"/> 后通过 <paramref name="transformer"/> 转换为 <typeparamref name="TNewResult"/> 。
        /// </summary>
        /// <param name="transformer">结果转换器。</param>
        /// <typeparam name="TNewResult">转换后结果对象类型。</typeparam>
        /// <returns>异步解释结果。</returns>
        Task<IResponse<TNewResult>> ResolveAsync<TNewResult>(Func<TResult, TNewResult> transformer)
            where TNewResult : class;
    }
}
