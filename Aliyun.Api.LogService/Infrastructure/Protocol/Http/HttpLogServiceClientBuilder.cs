//
// HttpLogServiceClientBuilder.cs
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
using System.Net;
using System.Net.Http;
using Aliyun.Api.LogService.Infrastructure.Authentication;
using Aliyun.Api.LogService.Utils;

namespace Aliyun.Api.LogService.Infrastructure.Protocol.Http
{
    public class HttpLogServiceClientBuilder
    {
        private Uri endpoint;

        private Func<Credential> credentialProvider;

        private Boolean? useProxy;

        private WebProxy proxy;

        private TimeSpan? timeout;

        /// <summary>
        /// 设置服务入口地址以及项目名称。
        /// </summary>
        /// <param name="endpoint">服务入口地址，协议部分可以忽略（支持如：end.point 或 http://end.point 或 https://end.point）。</param>
        /// <param name="project">项目名称。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder Endpoint(String endpoint, String project)
        {
            Ensure.NotEmpty(endpoint, nameof(endpoint));
            Ensure.NotEmpty(project, nameof(project));

            this.endpoint = parseUri(endpoint, project);

            return this;
        }

        private Uri parseUri(String endpoint, String project)
        {
            Uri uri;
            if (endpoint.StartsWith("http://"))
            {
                uri = new UriBuilder(endpoint.Insert(7, project + ".")).Uri;
            } else if (endpoint.StartsWith("https://"))
            {
                uri = new UriBuilder(endpoint.Insert(8, project + ".")).Uri;
            } else
            {
                uri = new UriBuilder("http://" + project + "." + endpoint).Uri;
            }

            return uri;
        }

        /// <summary>
        /// 设置服务入口地址以及项目名称。
        /// </summary>
        /// <param name="endpoint">服务入口地址。</param>
        /// <param name="project">项目名称。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder Endpoint(Uri endpoint, String project)
        {
            Ensure.NotNull(endpoint, nameof(endpoint));
            Ensure.NotEmpty(project, nameof(project));

            this.endpoint = parseUri(endpoint.Host, project);

            return this;
        }

        /// <summary>
        /// 设置服务使用的 accessKeyId 和 accessKey，此方法设置的凭据只能用于 RAM 模式下的主账号/子账号的访问。
        /// 跨账号访问需要使用 STS 模式并定时刷新凭据信息，<see cref="Credential(Func{Aliyun.Api.LogService.Infrastructure.Authentication.Credential})"/>。
        /// </summary>
        /// <param name="accessKeyId">accessKeyId。</param>
        /// <param name="accessKey">accessKey。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder Credential(String accessKeyId, String accessKey)
        {
            Ensure.NotEmpty(accessKeyId, nameof(accessKeyId));
            Ensure.NotEmpty(accessKey, nameof(accessKey));

            var credential = new Credential(accessKeyId, accessKey);
            this.credentialProvider = () => credential;

            return this;
        }

        /// <summary>
        /// 设置服务使用的 accessKeyId、 accessKey 及 stsToken 的动态提供者。
        /// 此方法设置的凭据支持 RAM 及 STS 模式。
        /// </summary>
        /// <param name="credentialProvider">凭据提供者。需要注意的是提供的 <paramref name="credentialProvider"/> 在每次请求中都会被调用，请注意缓存有效的服务凭据。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder Credential(Func<Credential> credentialProvider)
        {
            Ensure.NotNull(credentialProvider, nameof(credentialProvider));

            this.credentialProvider = credentialProvider;

            return this;
        }

        /// <summary>
        /// 设置连接是否使用代理。
        /// 如果无法确定是否需要使用，或想使用系统默认配置，请跳过此方法的调用即可。
        /// </summary>
        /// <param name="useProxy"><c>true</c> 表示使用代理，<c>false</c> 表示不使用代理（同时会跳过系统级别的代理设置）。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder UseProxy(Boolean useProxy)
        {
            this.useProxy = useProxy;

            return this;
        }

        /// <summary>
        /// 设置使用代理的信息。
        /// 如果 <see cref="UseProxy"/> 设置为 <c>false</c> 则此项设置无意义。
        /// </summary>
        /// <param name="proxyHost">代理地址（必填）。</param>
        /// <param name="proxyPort">代理端口（可选），不填则使用默认端口。</param>
        /// <param name="proxyUserName">代理验证用户（可选），不填则跳过验证。</param>
        /// <param name="proxyPassword">代理验证密码（可选），不填或者 <paramref name="proxyUserName"/> 为空时跳过验证。</param>
        /// <param name="proxyDomain">代理验证域（可选）。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder Proxy(String proxyHost,
            Int32? proxyPort = null, String proxyUserName = null, String proxyPassword = null, String proxyDomain = null)
        {
            Ensure.NotEmpty(proxyHost, nameof(proxyHost));

            var webProxy = proxyPort.HasValue
                ? new WebProxy(proxyHost, proxyPort.Value)
                : new WebProxy(proxyHost);

            if (proxyUserName.IsNotEmpty())
            {
                webProxy.Credentials = proxyDomain.IsEmpty()
                    ? new NetworkCredential(proxyUserName, proxyPassword ?? String.Empty)
                    : new NetworkCredential(proxyUserName, proxyPassword ?? String.Empty, proxyDomain);
            }

            this.proxy = webProxy;

            return this;
        }

        /// <summary>
        /// 设置请求超时时间，单位毫秒，需要使用其它单位的请使用 <see cref="RequestTimeout(TimeSpan)"/>。
        /// 如跳过此项设置，则使用底层框架的默认超时时间。
        /// </summary>
        /// <param name="timeoutMillis">请求超时时间，单位毫秒。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder RequestTimeout(Int32 timeoutMillis)
        {
            return this.RequestTimeout(TimeSpan.FromMilliseconds(timeoutMillis));
        }

        /// <summary>
        /// 设置请求超时时间。
        /// 如跳过此项设置，则使用底层框架的默认超时时间。
        /// </summary>
        /// <param name="timeout">请求超时时间。</param>
        /// <returns>当前实例。</returns>
        public HttpLogServiceClientBuilder RequestTimeout(TimeSpan timeout)
        {
            this.timeout = timeout;

            return this;
        }

        /// <summary>
        /// 根据上述设置构建 <see cref="HttpLogServiceClient"/>。
        /// </summary>
        /// <returns>构建好的 <see cref="HttpLogServiceClient"/>。</returns>
        public HttpLogServiceClient Build()
        {
            Ensure.NotNull(this.endpoint, nameof(this.endpoint));
            Ensure.NotNull(this.credentialProvider, nameof(this.credentialProvider));

            HttpClient httpClient;
            if (this.useProxy != null || this.proxy != null)
            {
                var httpClientHandler = new HttpClientHandler();

                if (this.useProxy != null)
                {
                    httpClientHandler.UseProxy = this.useProxy.Value;
                }

                if (this.proxy != null)
                {
                    httpClientHandler.Proxy = this.proxy;
                }

                httpClient = new HttpClient(httpClientHandler);
            } else
            {
                httpClient = new HttpClient();
            }

            httpClient.BaseAddress = this.endpoint;

            // Default use project scope api.
            httpClient.DefaultRequestHeaders.Host = this.endpoint.Host;

            if (this.timeout != null)
            {
                httpClient.Timeout = this.timeout.Value;
            }

            var client = new HttpLogServiceClient(httpClient, this.credentialProvider);

            return client;
        }
    }
}
