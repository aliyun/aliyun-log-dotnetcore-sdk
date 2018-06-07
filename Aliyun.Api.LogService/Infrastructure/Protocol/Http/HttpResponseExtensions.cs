//
// HttpResponseExtensions.cs
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
using System.Net;
using System.Net.Http;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    public static class HttpResponseExtensions
    {
        private static HttpResponse ToHttpResponse(this IResponse response)
            => response is HttpResponse httpResponse
                ? httpResponse
                : throw new ArgumentException($"response must be [{nameof(HttpResponse)}].", nameof(response));

        private static HttpResponse<T> ToHttpResponse<T>(this IResponse<T> response)
            where T : class
            => response is HttpResponse<T> httpResponse
                ? httpResponse
                : throw new ArgumentException($"response must be [{nameof(HttpResponse)}].", nameof(response));

        /// <summary>
        /// 获取此响应对象对应底层的 <see cref="HttpResponseMessage"/> 。
        /// </summary>
        /// <param name="response">响应对象。</param>
        /// <returns>底层的 <see cref="HttpResponseMessage"/> 。</returns>
        public static HttpResponseMessage GetHttpResponseMessage(this IResponse response)
        {
            return response.ToHttpResponse().ResponseMessage;
        }

        /// <summary>
        /// 获取此响应对象对应的 HTTP 响应码。
        /// </summary>
        /// <param name="response">响应对象。</param>
        /// <returns>HTTP 响应码。</returns>
        public static HttpStatusCode GetHttpStatusCode(this IResponse response)
        {
            return response.ToHttpResponse().StatusCode;
        }

        /// <summary>
        /// </summary>
        /// <param name="source">带有结果对象类型的响应消息解释器。</param>
        /// <param name="transformer">结果转换器。</param>
        /// <typeparam name="TSource">转换前结果对象类型。</typeparam>
        /// <typeparam name="TResult">转换后结果对象类型。</typeparam>
        /// <returns>异步解释结果。</returns>
        public static IResponse<TResult> Transform<TSource, TResult>(this IResponse<TSource> source,
            Func<IDictionary<String, String>, TSource, TResult> transformer)
            where TSource : class
            where TResult : class
        {
            var httpResponse = source.ToHttpResponse();
            var result = transformer(source.Headers, source.Result);
            var response = new HttpResponse<TResult>(httpResponse.ResponseMessage, httpResponse.IsSuccess, httpResponse.StatusCode, httpResponse.RequestId, httpResponse.Headers, result);
            return response;
        }
    }
}
