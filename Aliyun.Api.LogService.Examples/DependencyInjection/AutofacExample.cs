//
// AutofacExample.cs
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

using Autofac;

namespace Aliyun.Api.LogService.Examples.DependencyInjection
{
    public static class AutofacExample
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .Register(context => LogServiceClientBuilders.HttpBuilder
                    // 服务入口<endpoint>及项目名<projectName>
                    .Endpoint("<endpoint>", "<projectName>")
                    // 访问密钥信息
                    .Credential("<accessKeyId>", "<accessKey>")
                    .Build())
                // `ILogServiceClient`所有成员是线程安全的，建议使用Singleton模式。
                .SingleInstance();
        }
    }
}
