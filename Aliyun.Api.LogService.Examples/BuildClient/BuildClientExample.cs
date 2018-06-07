//
// BuildClientExample.cs
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

using System.Net.Http;
using Aliyun.Api.LogService.Infrastructure.Authentication;
using Aliyun.Api.LogService.Infrastructure.Protocol.Http;

namespace Aliyun.Api.LogService.Examples.BuildClient
{
    public static class BuildClientExample
    {
        /****************************************************************
         * `ILogServiceClient` 所有成员是线程安全的，建议使用Singleton模式。 *
         ****************************************************************/

        // 构建最简单的`ILogServiceClient`。
        public static ILogServiceClient BuildSimpleClient()
            => LogServiceClientBuilders.HttpBuilder
                // 服务入口<endpoint>及项目名<projectName>
                .Endpoint("<endpoint>", "<projectName>")
                // 访问密钥信息
                .Credential("<accessKeyId>", "<accessKey>")
                .Build();

        // 构建完整的`ILogServiceClient`。
        public static ILogServiceClient BuildFullClient()
            => LogServiceClientBuilders.HttpBuilder
                // 服务入口<endpoint>及项目名<projectName>。
                .Endpoint("<endpoint>", "<projectName>")
                // 访问密钥信息。
                .Credential("<accessKeyId>", "<accessKey>")
                // 设置每次请求超时时间。
                .RequestTimeout(1000)
                // 设置是否使用代理，为false时将会绕过系统代理。
                .UseProxy(true)
                // 设置代理信息，（可选）支持需要身份验证的代理设置。
                .Proxy("<proxyHost>", proxyUserName: "<username>", proxyPassword: "<password>")
                .Build();

        // 构建自定义的`ILogServiceClient`。
        public static ILogServiceClient BuildCustomClient()
            => new HttpLogServiceClient
            (
                // 支持自定义HttpClient实例
                new HttpClient(),
                // 访问密钥信息。
                () => new Credential("<accessKeyId>", "<accessKey>")
            );
    }
}
