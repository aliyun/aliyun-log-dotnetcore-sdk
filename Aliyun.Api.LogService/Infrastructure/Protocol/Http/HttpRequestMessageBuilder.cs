//
// HttpRequestMessageBuilder.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Aliyun.Api.LogService.Infrastructure.Authentication;
using Aliyun.Api.LogService.Utils;
using Google.Protobuf;
using Ionic.Zlib;
using LZ4;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    /// <summary>
    /// Builder for constructing the <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <inheritdoc />
    public class HttpRequestMessageBuilder : IRequestBuilder<HttpRequestMessage>
    {
        private static readonly Byte[] EmptyByteArray = new Byte[0];

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly HttpRequestMessage httpRequestMessage;

        private readonly Encoding encoding;

        private readonly String path;

        private readonly IDictionary<String, String> query;

        /// <summary>
        /// The authentication credential.
        /// </summary>
        private Credential credential;

        /// <summary>
        /// The real content to transfer. 
        /// </summary>
        private Object content;

        /// <summary>
        /// The Content-MD5 header in HEX format.
        /// </summary>
        private String contentMd5Hex;

        /// <summary>
        /// Gets the serialized content.
        /// </summary>
        private Byte[] SerializedContent =>
            this.content == null
                ? null
                : this.content as Byte[]
                  ?? throw new InvalidOperationException("Content must serialized before this operation.");

        /// <summary>
        /// Proceed the actions after content prepared (i.e., all transforms (e.g., serialize, compress, encrypt, encode) of <see cref="content"/> are applied). 
        /// </summary>
        private Action contentHandler;

        /// <summary>
        /// The signature type.
        /// </summary>
        private SignatureType signatureType;

        public HttpRequestMessageBuilder(HttpMethod method, String uri)
        {
            this.httpRequestMessage = new HttpRequestMessage(method, uri);
            this.encoding = Encoding.UTF8;
            ParseUri(uri, out this.path, out this.query);

            this.FillDefaultHeaders();
        }

        private static void ParseUri(String uri, out String path, out IDictionary<String, String> query)
        {
            var absUri = new Uri(new Uri("http://fa.ke"), uri);
            path = absUri.AbsolutePath;
            query = absUri.ParseQueryString()
                .ToEnumerable()
                .ToDictionary(kv => kv.Key, kv => kv.Value); // NOTE: Restricted mode, key cannot be duplicated.
        }

        private void FillDefaultHeaders()
        {
            this.httpRequestMessage.Headers.Date = DateTimeOffset.Now;
            this.httpRequestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("log-dotnetcore-sdk", Constants.AssemblyVersion));
            this.httpRequestMessage.Headers.Add(LogHeaders.ApiVersion, "0.6.0");
        }

        #region Query

        public IRequestBuilder<HttpRequestMessage> Query(String key, String value)
        {
            this.query.Add(key, value);
            return this;
        }

        public IRequestBuilder<HttpRequestMessage> Query(Object queryModel)
        {
            foreach (var kv in JObject.FromObject(queryModel, JsonSerializer.CreateDefault(JsonSerializerSettings)))
            {
                this.query.Add(kv.Key, kv.Value.Value<String>());
            }

            return this;
        }

        #endregion


        #region Header

        /// <summary>
        /// Set headers of <see cref="T:System.Net.Http.Headers.HttpRequestHeaders" />
        /// </summary>
        /// <inheritdoc />
        public IRequestBuilder<HttpRequestMessage> Header(String key, String value)
        {
            this.httpRequestMessage.Headers.Add(key, value);

            return this;
        }

        private void ContentHeader(Action<HttpContentHeaders> option)
        {
            if (this.httpRequestMessage.Content == null)
            {
                this.contentHandler += () => option(this.httpRequestMessage.Content.Headers);
            } else
            {
                option(this.httpRequestMessage.Content.Headers);
            }
        }

        private void SetBodyRawSize(Int32 size)
            => this.httpRequestMessage.Headers.Add(LogHeaders.BodyRawSize, size.ToString());

        private void SetCompressType(String compressType)
            => this.httpRequestMessage.Headers.Add(LogHeaders.CompressType, compressType);

        private void SetSignatureMethod(String signatureMethod)
            => this.httpRequestMessage.Headers.Add(LogHeaders.SignatureMethod, signatureMethod);

        #endregion


        #region Content

        public IRequestBuilder<HttpRequestMessage> Content(Byte[] content)
            => this.Content((Object) content);

        public IRequestBuilder<HttpRequestMessage> Content(Object content)
        {
            this.content = content;

            if (content is Byte[] data)
            {
                this.SetBodyRawSize(data.Length);
            }

            return this;
        }

        #endregion


        #region Serialize

        public IRequestBuilder<HttpRequestMessage> Serialize(SerializeType serializeType)
        {
            switch (this.content)
            {
                case null:
                    throw new InvalidOperationException("Nothing to serialize.");
                case Byte[] _:
                    throw new InvalidOperationException("Content has already been serialized.");
            }

            switch (serializeType)
            {
                case SerializeType.Json:
                {
                    this.ContentHeader(x => x.ContentType = new MediaTypeHeaderValue("application/json"));
                    var json = JsonConvert.SerializeObject(this.content, JsonSerializerSettings);
                    this.Content(this.encoding.GetBytes(json));

                    break;
                }

                case SerializeType.Protobuf:
                {
                    if (!(this.content is IMessage protoMessage))
                    {
                        throw new ArgumentException("Serialization of ProtoBuf requires IMessage.");
                    }

                    this.ContentHeader(x => x.ContentType = new MediaTypeHeaderValue("application/x-protobuf"));
                    this.Content(protoMessage.ToByteArray());

                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(serializeType), serializeType, null);
                }
            }

            return this;
        }

        #endregion Serialize


        #region Compress

        public IRequestBuilder<HttpRequestMessage> Compress(CompressType compressType)
        {
            if (this.SerializedContent == null)
            {
                throw new InvalidOperationException("Nothing to compress.");
            }

            switch (compressType)
            {
                case CompressType.None:
                {
                    break;
                }

                case CompressType.Lz4:
                {
                    this.SetCompressType("lz4");
                    this.content = LZ4Codec.Encode(this.SerializedContent, 0, this.SerializedContent.Length);
                    break;
                }

                case CompressType.Deflate:
                {
                    this.SetCompressType("deflate");
                    this.content = ZlibStream.CompressBuffer(this.SerializedContent);
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(compressType), compressType, null);
                }
            }

            return this;
        }

        #endregion Compress


        #region Authentication

        public IRequestBuilder<HttpRequestMessage> Authenticate(Credential credential)
        {
            Ensure.NotNull(credential, nameof(credential));
            Ensure.NotEmpty(credential.AccessKeyId, nameof(credential.AccessKeyId));
            Ensure.NotEmpty(credential.AccessKey, nameof(credential.AccessKey));

            this.credential = credential;
            return this;
        }

        #endregion


        #region Sign

        public IRequestBuilder<HttpRequestMessage> Sign(SignatureType signatureType)
        {
            this.signatureType = signatureType;
            return this;
        }

        private Byte[] ComputeSignature()
        {
            switch (this.signatureType)
            {
                case SignatureType.HmacSha1:
                {
                    using (var hasher = new HMACSHA1(this.encoding.GetBytes(this.credential.AccessKey)))
                    {
                        this.SetSignatureMethod("hmac-sha1"); // This header must be set before generating sign source.
                        var signSource = this.GenerateSignSource();
                        var sign = hasher.ComputeHash(this.encoding.GetBytes(signSource));

                        return sign;
                    }
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(this.signatureType), this.signatureType, "Currently only support [hmac-sha1] signature.");
                }
            }
        }

        private String GenerateSignSource()
        {
            var verb = this.httpRequestMessage.Method.Method;
            var contentMd5 = this.contentMd5Hex;
            var contentType = this.httpRequestMessage.Content?.Headers.ContentType.MediaType;
            var date = this.httpRequestMessage.Headers.Date?.ToString("r"); /* RFC 822 format */
            var logHeaders = String.Join("\n", this.httpRequestMessage.Headers
                .Concat(this.httpRequestMessage.Content?.Headers ?? Enumerable.Empty<KeyValuePair<String, IEnumerable<String>>>())
                .Where(x => x.Key.StartsWith("x-log") || x.Key.StartsWith("x-acs"))
                .Select(x => new KeyValuePair<String, String>(x.Key.ToLower(), x.Value.SingleOrDefault() /* Fault tolerance */))
                .Where(x => x.Value.IsNotEmpty()) // Remove empty header
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}:{x.Value}"));

            var resource = this.httpRequestMessage.RequestUri.OriginalString;

            String signSource;
            if (this.query.IsEmpty())
            {
                signSource = String.Join("\n", verb, contentMd5 ?? String.Empty, contentType ?? String.Empty, date, logHeaders, resource);
            } else
            {
                signSource = String.Join("\n", verb, contentMd5 ?? String.Empty, contentType ?? String.Empty, date, logHeaders, resource) + "?" +
                             String.Join("&", this.query
                                 .OrderBy(x => x.Key)
                                 .Select(x => $"{x.Key}={x.Value}"));
            }
            return signSource;
        }

        private Byte[] CalculateContentMd5()
        {
            using (var hasher = MD5.Create())
            {
                return hasher.ComputeHash(this.SerializedContent);
            }
        }

        #endregion Signature

        public HttpRequestMessage Build()
        {
            // Validate
            Ensure.NotNull(this.credential, nameof(this.credential));
            Ensure.NotEmpty(this.credential.AccessKeyId, nameof(this.credential.AccessKeyId));
            Ensure.NotEmpty(this.credential.AccessKey, nameof(this.credential.AccessKey));

            // Process sts-token.
            var hasSecurityToken = this.httpRequestMessage.Headers.TryGetValues(LogHeaders.SecurityToken, out var securityTokens)
                                   && securityTokens.FirstOrDefault().IsNotEmpty();

            if (!hasSecurityToken && this.credential.StsToken.IsNotEmpty())
            {
                this.httpRequestMessage.Headers.Add(LogHeaders.SecurityToken, this.credential.StsToken);
            }

            // NOTE: If x-log-bodyrawsize is empty, fill it with "0". Otherwise, some method call will be corrupted.
            if (!this.httpRequestMessage.Headers.Contains(LogHeaders.BodyRawSize))
            {
                this.SetBodyRawSize(0);
            }

            // Build content if necessary
            if (this.SerializedContent.IsNotEmpty())
            {
                this.httpRequestMessage.Content = new ByteArrayContent(this.SerializedContent);
                this.contentHandler?.Invoke();

                // Prepare header
                this.ContentHeader(x =>
                {
                    // Compute actual length
                    x.ContentLength = this.SerializedContent.Length;
                    // Compute actual MD5
                    this.contentMd5Hex = BitConverter.ToString(this.CalculateContentMd5()).Replace("-", String.Empty);

                    x.Add("Content-MD5", this.contentMd5Hex); // Non-standard header
                });
            } else if (this.httpRequestMessage.Method == HttpMethod.Post || this.httpRequestMessage.Method == HttpMethod.Put)
            {
                // When content is empty as well as method is `POST` or `PUT`, generate an empty content and corresponding headers.

                this.httpRequestMessage.Content = new ByteArrayContent(EmptyByteArray);
                // Don't invoke `contentHandler` here!

                /*
                 * NOTE:
                 * Here is a annoying hack, the log service service cannot accept empty `Content-Type`
                 * header when POST or PUT methods. So, we have to force set some header value.
                 */
                this.ContentHeader(x =>
                {
                    x.ContentType = new MediaTypeHeaderValue("application/json");
                    // For some reason, I think it is better to set `Content-Type` to `0` to prevent
                    // some unexpected behavior on server side.
                    x.ContentLength = 0;
                });
            }

            // Do signature
            var signature = Convert.ToBase64String(this.ComputeSignature());
            this.httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("LOG", $"{this.credential.AccessKeyId}:{signature}");

            // Rebuild the RequestUri
            var queryString = String.Join("&", this.query
                .OrderBy(x => x.Key)
                .Select(x => $"{encodeUrl(x.Key)}={encodeUrl(x.Value)}"));
            var pathAndQuery = queryString.IsNotEmpty() ? $"{this.path}?{queryString}" : this.path;
            this.httpRequestMessage.RequestUri = new Uri(pathAndQuery, UriKind.Relative);

            return this.httpRequestMessage;
        }

        private String encodeUrl(String value)
        {
            if (value == null)
            {
                return "";
            }

            string encoded = HttpUtility.UrlEncode(value, this.encoding);
            return encoded.Replace("+", "%20").Replace("*", "%2A").Replace("~", "%7E").Replace("/", "%2F");
        }
    }
}
