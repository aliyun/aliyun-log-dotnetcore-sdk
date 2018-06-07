//
// IRequestBuilder.cs
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
using System.Net.Http;
using Aliyun.Api.LogService.Infrastructure.Authentication;

namespace Aliyun.Api.LogService.Infrastructure.Protocol
{
    /// <summary>
    /// The builder for constructing request <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of building request.</typeparam>
    public interface IRequestBuilder<out T>
    {
        /// <summary>
        /// Adds query of request. 
        /// </summary>
        /// <param name="key">The key of query.</param>
        /// <param name="value">The value of query.</param>
        /// <returns>This builder.</returns>
        /// <exception cref="ArgumentException"><paramref name="key"/> has already been exist.</exception>
        IRequestBuilder<T> Query(String key, String value);

        /// <summary>
        /// Add all public gettable properties of <paramref name="queryModel"/> to query.
        /// </summary>
        /// <param name="queryModel">A simple data object with no nested complex properties.</param>
        /// <returns>This builder.</returns>
        /// <exception cref="InvalidCastException">If <paramref name="queryModel"/> has nested complex properties.</exception>
        IRequestBuilder<T> Query(Object queryModel);

        /// <summary>
        /// Adds header of request. 
        /// </summary>
        /// <param name="key">The key of header.</param>
        /// <param name="value">The value of header.</param>
        /// <returns>This builder.</returns>
        IRequestBuilder<T> Header(String key, String value);

        /// <summary>
        /// Sets serialized content of request.
        /// </summary>
        /// <param name="content">The serialized content.</param>
        /// <returns>This builder.</returns>
        /// <seealso cref="Content(Object)"/>
        IRequestBuilder<T> Content(Byte[] content);

        /// <summary>
        /// Sets content of request.
        /// </summary>
        /// <param name="content">The content, can be serialized or not.</param>
        /// <returns>This builder.</returns>
        IRequestBuilder<T> Content(Object content);

        /// <summary>
        /// Serialize the content.
        /// </summary>
        /// <param name="serializeType">The serialze type</param>
        /// <returns>This builder.</returns>
        /// <exception cref="InvalidOperationException">If nothing to serialize, or content has already been serialized.</exception>
        IRequestBuilder<T> Serialize(SerializeType serializeType);

        /// <summary>
        /// Compress the content.
        /// </summary>
        /// <param name="compressType">The compress type</param>
        /// <returns>This builder.</returns>
        /// <exception cref="InvalidOperationException">If nothing to compress, or content is not serialized.</exception>
        IRequestBuilder<T> Compress(CompressType compressType);

        /// <summary>
        /// Set credential to authenticate.
        /// </summary>
        /// <param name="credential">The authenticate credential.</param>
        /// <returns></returns>
        IRequestBuilder<HttpRequestMessage> Authenticate(Credential credential);

        /// <summary>
        /// Specify the signature type and credentials to sign.
        /// </summary>
        /// <param name="signatureType">The signature type</param>
        /// <returns>This builder.</returns>
        IRequestBuilder<T> Sign(SignatureType signatureType);

        /// <summary>
        /// Build the request <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The built request.</returns>
        T Build();
    }
}
