//
// LogServiceClientBuilders.cs
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
using Aliyun.Api.LogService.Infrastructure.Protocol;

namespace Aliyun.Api.LogService.Domain
{
    /// <summary>
    /// 日志服务业务异常。
    /// </summary>
    public class LogServiceException : Exception
    {
        /// <summary>
        /// 服务端产生的标示该请求的唯一 ID。
        /// 该响应头与具体应用无关，主要用于跟踪和调查问题。
        /// 如果用户希望调查出现问题的 API 请求，可以向 Log Service 团队提供该 ID。
        /// </summary>
        public String RequestId { get; }

        /// <summary>
        /// 对应的错误码。
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <summary>
        /// 对应的错误消息。
        /// </summary>
        public String ErrorMessage { get; }

        public LogServiceException(String requestId, ErrorCode errorCode)
            : base(FormatMessage(requestId, errorCode))
        {
            this.RequestId = requestId;
            this.ErrorCode = errorCode;
            this.ErrorMessage = null;
        }

        public LogServiceException(String requestId, ErrorCode errorCode, String errorMessage)
            : base(FormatMessage(requestId, errorCode, errorMessage))
        {
            this.RequestId = requestId;
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
        }

        public LogServiceException(String requestId, ErrorCode errorCode, Exception innerException)
            : base(FormatMessage(requestId, errorCode), innerException)
        {
            this.RequestId = requestId;
            this.ErrorCode = errorCode;
            this.ErrorMessage = null;
        }

        public LogServiceException(String requestId, ErrorCode errorCode, String errorMessage, Exception innerException)
            : base(FormatMessage(requestId, errorCode), innerException)
        {
            this.RequestId = requestId;
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
        }

        private static String FormatMessage(String requestId, ErrorCode errorCode, String errorMessage = null)
            => $"[{requestId}] {errorCode}{(errorMessage == null ? String.Empty : $" ({errorMessage})")}";
    }
}
