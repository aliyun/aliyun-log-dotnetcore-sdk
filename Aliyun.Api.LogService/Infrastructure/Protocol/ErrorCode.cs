//
// ErrorCode.cs
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
using System.Reflection;

namespace Aliyun.Api.LogService.Infrastructure.Protocol
{
    public class ErrorCode
    {
        #region Known error codes

        /// <summary>
        /// SDK 内部错误。
        /// </summary>
        internal static ErrorCode SdkInternalError { get; } = new ErrorCode(nameof(SdkInternalError));

        ///<summary>
        ///没有提供必须的 Content-Length 请求头。
        ///</summary>
        public static ErrorCode MissingContentLength { get; } = new ErrorCode(nameof(MissingContentLength));

        ///<summary>
        ///不支持 Content-Type 指定的类型。
        ///</summary>
        public static ErrorCode InvalidContentType { get; } = new ErrorCode(nameof(InvalidContentType));

        ///<summary>
        ///没有为 Body 不为空的 HTTP 请求指定 Content-Type 头。
        ///</summary>
        public static ErrorCode MissingContentType { get; } = new ErrorCode(nameof(MissingContentType));

        ///<summary>
        ///压缩场景下没有提供必须的 x-log-bodyrawsize 请求头。
        ///</summary>
        public static ErrorCode MissingBodyRawSize { get; } = new ErrorCode(nameof(MissingBodyRawSize));

        ///<summary>
        ///x-log-bodyrawsize 的值无效。
        ///</summary>
        public static ErrorCode InvalidBodyRawSize { get; } = new ErrorCode(nameof(InvalidBodyRawSize));

        ///<summary>
        ///x-log-compresstype 指定的压缩方式不支持。
        ///</summary>
        public static ErrorCode InvalidCompressType { get; } = new ErrorCode(nameof(InvalidCompressType));

        ///<summary>
        ///没有提供 HTTP 标准请求头 Host。
        ///</summary>
        public static ErrorCode MissingHost { get; } = new ErrorCode(nameof(MissingHost));

        ///<summary>
        ///没有提供 HTTP 标准请求头 Date。
        ///</summary>
        public static ErrorCode MissingDate { get; } = new ErrorCode(nameof(MissingDate));

        ///<summary>
        ///Date 请求头的值不符合 RFC822 标准。
        ///</summary>
        public static ErrorCode InvalidDateFormat { get; } = new ErrorCode(nameof(InvalidDateFormat));

        ///<summary>
        ///没有提供 HTTP 请求头 x-log-apiversion。
        ///</summary>
        public static ErrorCode MissingAPIVersion { get; } = new ErrorCode(nameof(MissingAPIVersion));

        ///<summary>
        ///HTTP 请求头 x-log-apiversion 的值不支持。
        ///</summary>
        public static ErrorCode InvalidAPIVersion { get; } = new ErrorCode(nameof(InvalidAPIVersion));

        ///<summary>
        ///没有在 Authorization 头部提供 AccessKeyId。
        ///</summary>
        public static ErrorCode MissAccessKeyId { get; } = new ErrorCode(nameof(MissAccessKeyId));

        ///<summary>
        ///提供的 AccessKeyId 值未授权。
        ///</summary>
        public static ErrorCode Unauthorized { get; } = new ErrorCode(nameof(Unauthorized));

        ///<summary>
        ///没有提供 HTTP 请求头 x-log-signaturemethod。
        ///</summary>
        public static ErrorCode MissingSignatureMethod { get; } = new ErrorCode(nameof(MissingSignatureMethod));

        ///<summary>
        ///x-log-signaturemethod 头部指定的签名方法不支持。
        ///</summary>
        public static ErrorCode InvalidSignatureMethod { get; } = new ErrorCode(nameof(InvalidSignatureMethod));

        ///<summary>
        ///请求的发送时间超过当前服务处理时间前后 15 分钟的范围。
        ///</summary>
        public static ErrorCode RequestTimeTooSkewed { get; } = new ErrorCode(nameof(RequestTimeTooSkewed));

        ///<summary>
        ///日志项目（Project）不存在。
        ///</summary>
        public static ErrorCode ProjectNotExist { get; } = new ErrorCode(nameof(ProjectNotExist));

        ///<summary>
        ///请求的数字签名不匹配。
        ///</summary>
        public static ErrorCode SignatureNotMatch { get; } = new ErrorCode(nameof(SignatureNotMatch));

        ///<summary>
        ///超过写入日志限额。
        ///</summary>
        public static ErrorCode WriteQuotaExceed { get; } = new ErrorCode(nameof(WriteQuotaExceed));

        ///<summary>
        ///超过读取日志限额。
        ///</summary>
        public static ErrorCode ReadQuotaExceed { get; } = new ErrorCode(nameof(ReadQuotaExceed));

        ///<summary>
        ///服务器内部错误。
        ///</summary>
        public static ErrorCode InternalServerError { get; } = new ErrorCode(nameof(InternalServerError));

        ///<summary>
        ///服务器正忙，请稍后再试。
        ///</summary>
        public static ErrorCode ServerBusy { get; } = new ErrorCode(nameof(ServerBusy));

        #endregion

        private static readonly IDictionary<String, ErrorCode> KnownErrorCodes;

        public String Code { get; }

        static ErrorCode()
        {
            KnownErrorCodes = new Dictionary<String, ErrorCode>(typeof(ErrorCode)
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(x => x.PropertyType == typeof(ErrorCode))
                .Select(x => x.GetValue(null))
                .Cast<ErrorCode>()
                .ToDictionary(x => x.Code, x => x));
        }

        private ErrorCode(String code)
        {
            this.Code = code;
        }

        public static Boolean IsKnownErrorCode(String code)
        {
            return KnownErrorCodes.ContainsKey(code);
        }

        public Boolean IsKnownErrorCode()
        {
            return IsKnownErrorCode(this.Code);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            switch (obj)
            {
                case ErrorCode other:
                    return String.Equals(this.Code, other.Code);
                case String other:
                    return String.Equals(this.Code, other);
                default:
                    return false;
            }
        }

        public override Int32 GetHashCode()
            => this.Code?.GetHashCode() ?? 0;

        public override String ToString()
            => this.Code;

        public static implicit operator String(ErrorCode code)
            => code?.Code;

        public static implicit operator ErrorCode(String code)
            => code == null
                ? null
                : (KnownErrorCodes.TryGetValue(code, out var errorCode)
                    ? errorCode
                    : new ErrorCode(code));

        public static Boolean operator ==(ErrorCode lhs, ErrorCode rhs)
            => lhs?.Code == rhs?.Code;

        public static Boolean operator !=(ErrorCode lhs, ErrorCode rhs)
            => lhs?.Code != rhs?.Code;
    }
}
