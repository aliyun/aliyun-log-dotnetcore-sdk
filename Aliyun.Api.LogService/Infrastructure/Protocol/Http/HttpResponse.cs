//
// HttpResponse.cs
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Domain;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    /// <summary>
    /// 服务响应包装对象，包含未反序列化的原始数据，可通过 <c>ReadXxxAsync()</c> 方法读取原始报文。
    /// </summary>
    public class HttpResponse : IResponse
    {
        internal HttpResponseMessage ResponseMessage { get; }

        public Boolean IsSuccess { get; }

        public HttpStatusCode StatusCode { get; }

        public String RequestId { get; }

        public IDictionary<String, String> Headers { get; }

        public Error Error { get; }

        public HttpResponse(HttpResponseMessage responseMessage, Boolean isSuccess, HttpStatusCode statusCode, String requestId, IDictionary<String, String> headers, Error error)
        {
            this.ResponseMessage = responseMessage;
            this.IsSuccess = isSuccess;
            this.StatusCode = statusCode;
            this.RequestId = requestId;
            this.Headers = headers;
            this.Error = error;
        }

        public IResponse EnsureSuccess()
        {
            if (!this.IsSuccess)
            {
                throw this.Error == null
                    ? new LogServiceException(this.RequestId, ErrorCode.SdkInternalError, "The error detail result is missing.")
                    : new LogServiceException(this.RequestId, this.Error.ErrorCode, this.Error.ErrorMessage);
            }

            return this;
        }

        public Task<TResult> ReadAsAsync<TResult>()
        {
            return this.ResponseMessage.Content.ReadAsAsync<TResult>();
        }

        public Task<Stream> ReadAsByteStreamAsync()
        {
            return this.ResponseMessage.Content.ReadAsStreamAsync();
        }

        public Task<Byte[]> ReadAsByteArrayAsync()
        {
            return this.ResponseMessage.Content.ReadAsByteArrayAsync();
        }

        public override String ToString()
            => $"[{this.RequestId}] {this.StatusCode}{(this.IsSuccess ? String.Empty : " Error:" + this.Error)}";
    }

    /// <summary>
    /// 服务响应包装对象，此类型包含一个已反序列化为 <typeparamref name="TResult" /> 的 <see cref="Result">结果对象</see>。
    /// </summary>
    /// <typeparam name="TResult">响应包含结果的类型。</typeparam>
    public class HttpResponse<TResult> : HttpResponse, IResponse<TResult>
        where TResult : class
    {
        public TResult Result { get; }

        public HttpResponse(HttpResponseMessage responseMessage, Boolean isSuccess, HttpStatusCode statusCode, String requestId, IDictionary<String, String> headers, Error error)
            : base(responseMessage, isSuccess, statusCode, requestId, headers, error)
        {
            this.Result = null;
        }

        public HttpResponse(HttpResponseMessage responseMessage, Boolean isSuccess, HttpStatusCode statusCode, String requestId, IDictionary<String, String> headers, TResult result)
            : base(responseMessage, isSuccess, statusCode, requestId, headers, null)
        {
            this.Result = result;
        }

        IResponse<TResult> IResponse<TResult>.EnsureSuccess()
        {
            this.EnsureSuccess();
            return this;
        }

        public override String ToString()
            => base.ToString() + $" Result:{(this.Result == null ? "<null>" : this.Result.GetType().FullName)}";
    }
}
