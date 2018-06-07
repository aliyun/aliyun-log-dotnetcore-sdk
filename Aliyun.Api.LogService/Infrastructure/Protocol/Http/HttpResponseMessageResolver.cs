//
// HttpResponseMessageResolver.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Aliyun.Api.LogService.Utils;
using Ionic.Zlib;
using LZ4;
using Newtonsoft.Json;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    public class HttpResponseMessageResolver : IResponseResolver
    {
        public HttpResponseMessage HttpResponseMessage { get; }
        
        public String RequestId { get; private set; }

        public Boolean IsSuccess { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public IDictionary<String, String> Headers { get; private set; }

        private Func<Byte[], Byte[]> decompressor;

        private Func<Byte[], Type, Object> deserializer;

        public HttpResponseMessageResolver(HttpResponseMessage httpResponseMessage)
        {
            this.HttpResponseMessage = httpResponseMessage;

            this.decompressor = this.AutoDecompressContent;
            this.deserializer = this.AutoDeserializeContent;
        }

        public static IResponseResolver For(HttpResponseMessage httpResponseMessage)
        {
            return new HttpResponseMessageResolver(httpResponseMessage);
        }

        public static IResponseResolver<TResult> For<TResult>(HttpResponseMessage httpResponseMessage)
            where TResult : class
        {
            return new HttpResponseMessageResolver(httpResponseMessage).With<TResult>();
        }

        public IResponseResolver<TResult> With<TResult>()
            where TResult : class
        {
            return new TypedWrapper<TResult>(this);
        }

        public IResponseResolver Decompress(Func<Byte[], Byte[]> decompressor)
        {
            this.decompressor = decompressor ?? throw new ArgumentNullException(nameof(decompressor));
            return this;
        }

        public IResponseResolver<TResult> Deserialize<TResult>(Func<Byte[], TResult> deserializer) where TResult : class
        {
            if (deserializer == null)
            {
                throw new ArgumentNullException(nameof(deserializer));
            }

            this.deserializer = (data, resultType) =>
            {
                var bindType = typeof(TResult);
                if (bindType != resultType)
                {
                    throw new ArgumentException($"Type mismatch, binding type: [{bindType}], actual type: [{resultType}]", nameof(TResult));
                }

                return deserializer(data);
            };

            return new TypedWrapper<TResult>(this);
        }


        #region Decompress

        private Byte[] AutoDecompressContent(Byte[] data)
        {
            // Try decompress data if necessary
            if (this.TryGetCompressTypeHeader(out var compressType))
            {
                var optionalBodyRawSize = this.GetOptionalBodyRawSizeHeader();
                // Replace the data
                return DecompressContent(compressType, data, optionalBodyRawSize);
            }

            return data;
        }

        private Boolean TryGetCompressTypeHeader(out CompressType compressType)
        {
            if (!this.HttpResponseMessage.Headers.TryGetValues(LogHeaders.CompressType, out var compressTypes))
            {
                // No header
                compressType = CompressType.None;
                return false;
            }

            var compressTypeValue = compressTypes.FirstOrDefault(); // Fault tolerance (TODO: Show warns about duplicated keys)
            if (compressTypeValue.IsEmpty())
            {
                // Header is empty
                compressType = CompressType.None;
                return false;
            }

            // Convert value to enum
            return Enum.TryParse(compressTypeValue, true, out compressType)
                ? true
                : throw new ArgumentException($"Compress type [{compressTypeValue}] is not supported.", LogHeaders.CompressType);
        }

        private Int32? GetOptionalBodyRawSizeHeader()
        {
            if (!this.HttpResponseMessage.Headers.TryGetValues(LogHeaders.BodyRawSize, out var bodyRawSizes))
            {
                // No header
                return null;
            }

            var bodyRawSizeValue = bodyRawSizes.FirstOrDefault(); // Fault tolerance (TODO: Show warns about duplicated keys)
            if (bodyRawSizeValue.IsEmpty())
            {
                // Header is empty
                return null;
            }

            return Int32.Parse(bodyRawSizeValue); // Let exception raise when format is incorrect.
        }

        private static Byte[] DecompressContent(CompressType compressType, Byte[] orignData, Int32? rawSize)
        {
            switch (compressType)
            {
                case CompressType.None:
                {
                    return orignData;
                }

                case CompressType.Lz4:
                {
                    if (!rawSize.HasValue)
                    {
                        throw new ArgumentException($"{LogHeaders.BodyRawSize} is required when using [lz4] compress.");
                    }

                    var rawData = LZ4Codec.Decode(orignData, 0, orignData.Length, rawSize.Value);
                    return rawData;
                }

                case CompressType.Deflate:
                {
                    var rawData = ZlibStream.UncompressBuffer(orignData);
                    return rawData;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(compressType), compressType, null);
                }
            }
        }

        #endregion Decompress

        #region Deserialize

        private Object AutoDeserializeContent(Byte[] data, Type resultType)
        {
            // Content negotiate is not supported.
            using (var stream = new MemoryStream(data, false))
            using (var textReader = new StreamReader(stream, Encoding.UTF8 /*TODO: Hard code*/))
            {
                return JsonSerializer.CreateDefault().Deserialize(textReader, resultType);
            }
        }

        #endregion

        private void ResolveInternal()
        {
            if (this.HttpResponseMessage.Headers.TryGetValues(LogHeaders.RequestId, out var requestIds))
            {
                this.RequestId = requestIds.FirstOrDefault(); // Fault tolerance.
            }
            this.IsSuccess = this.HttpResponseMessage.IsSuccessStatusCode;
            this.StatusCode = this.HttpResponseMessage.StatusCode;

            this.Headers = this.HttpResponseMessage.Headers
                .Concat(this.HttpResponseMessage.Content.Headers ?? Enumerable.Empty<KeyValuePair<String, IEnumerable<String>>>())
                .ToDictionary(kv => kv.Key, kv => kv.Value.FirstOrDefault() /* Fault tolerance */);
        }

        private async Task<TResult> ResolveResultAsync<TResult>()
            where TResult : class
        {
            var httpContent = this.HttpResponseMessage.Content;
            if (httpContent == null)
            {
                return null;
            }

            var data = await httpContent.ReadAsByteArrayAsync();
            if (data.IsEmpty())
            {
                return null;
            }

            data = this.decompressor(data);
            var result = this.deserializer(data, typeof(TResult));

            return (TResult) result; // Always safe! Expect the custom serializer does some weird operations.
        }

        public async Task<IResponse> ResolveAsync()
        {
            this.ResolveInternal();

            var readOnlyHeaders = new ReadOnlyDictionary<String, String>(this.Headers);

            var error = this.IsSuccess ? null : await this.HttpResponseMessage.Content.ReadAsAsync<Error>();

            return new HttpResponse(this.HttpResponseMessage, this.IsSuccess, this.StatusCode, this.RequestId, readOnlyHeaders, error);
        }

        public async Task<IResponse<TResult>> ResolveAsync<TResult>() where TResult : class
        {
            this.ResolveInternal();

            var readOnlyHeaders = new ReadOnlyDictionary<String, String>(this.Headers);

            if (!this.IsSuccess)
            {
                var error = await this.HttpResponseMessage.Content.ReadAsAsync<Error>();
                return new HttpResponse<TResult>(this.HttpResponseMessage, this.IsSuccess, this.StatusCode, this.RequestId, readOnlyHeaders, error);
            }

            var result = await this.ResolveResultAsync<TResult>();

            return new HttpResponse<TResult>(this.HttpResponseMessage, this.IsSuccess, this.StatusCode, this.RequestId, readOnlyHeaders, result);
        }

        private class TypedWrapper<TResult> : IResponseResolver<TResult>
            where TResult : class
        {
            private readonly HttpResponseMessageResolver innerResolver;

            internal TypedWrapper(HttpResponseMessageResolver innerResolver)
            {
                this.innerResolver = innerResolver;
            }

            public IResponseResolver<TResult> Decompress(Func<Byte[], Byte[]> decompressor)
            {
                this.innerResolver.Decompress(decompressor);
                return this;
            }

            public IResponseResolver<TResult> Deserialize(Func<Byte[], TResult> deserializer)
            {
                this.innerResolver.Deserialize(deserializer);
                return this;
            }

            public Task<IResponse<TResult>> ResolveAsync()
            {
                return this.innerResolver.ResolveAsync<TResult>();
            }

            public async Task<IResponse<TNewResult>> ResolveAsync<TNewResult>(Func<TResult, TNewResult> transformer)
                where TNewResult : class
            {
                this.innerResolver.ResolveInternal();

                var readOnlyHeaders = new ReadOnlyDictionary<String, String>(this.innerResolver.Headers);

                if (!this.innerResolver.IsSuccess)
                {
                    var error = await this.innerResolver.HttpResponseMessage.Content.ReadAsAsync<Error>();
                    return new HttpResponse<TNewResult>(this.innerResolver.HttpResponseMessage, this.innerResolver.IsSuccess, this.innerResolver.StatusCode, this.innerResolver.RequestId, readOnlyHeaders, error);
                }

                var result = await this.innerResolver.ResolveResultAsync<TResult>();
                var newResult = transformer(result);

                return new HttpResponse<TNewResult>(this.innerResolver.HttpResponseMessage, this.innerResolver.IsSuccess, this.innerResolver.StatusCode, this.innerResolver.RequestId, readOnlyHeaders, newResult);
            }
        }
    }
}
