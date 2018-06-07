//
// BuildClientUsingStsTokenExample.cs
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
using System.Timers;
using Aliyun.Api.LogService.Infrastructure.Authentication;

namespace Aliyun.Api.LogService.Examples.BuildClient
{
    public static class BuildClientUsingStsTokenExample
    {
        private static readonly CredentialHolder Holder = new CredentialHolder();

        /// <summary>
        /// 创建一个使用 STS Token 的 Client 。
        /// </summary>
        public static ILogServiceClient BuildClientUsingStsToken()
        {
            var client = LogServiceClientBuilders.HttpBuilder
                // 服务入口<endpoint>及项目名<projectName>
                .Endpoint("<endpoint>", "<projectName>")
                // 服务凭据
                // 请注意此处提供的是委托，而非变量，此委托会在**每次执行**时被调用，请务必注意性能问题！
                .Credential(() => Holder.Credential)
                .Build();

            // 在 Client 创建完成后，务必保留定时任务以便定时刷新 Credential 。

            return client;
        }

        /// <summary>
        /// 凭据的定时刷新器。
        /// </summary>
        private class CredentialHolder : IDisposable
        {
            private readonly Timer timer;

            internal Credential Credential { get; private set; }

            public CredentialHolder()
            {
                // 由于 STS Token 时效性短，需要定时刷新。
                this.timer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds); // 每10分钟执行
                this.timer.Elapsed += (sender, args) => this.Refresh(); // 定时执行的任务
                this.timer.Start();
            }

            /// <summary>
            /// 刷新凭据。
            /// </summary>
            public void Refresh()
            {
                // 从 STS 服务获取凭据信息，此调用假定有返回值时总是成功，在失败时抛出异常。
                var (accessKeyId, accessKey, stsToken) = this.AssumeRoleOrThrow();

                // 更新服务凭据
                this.Credential = new Credential(accessKeyId, accessKey, stsToken);
            }

            /// <summary>
            /// 获取远程凭据，例子中未实现。
            /// 此调用假定有返回值时总是成功，在失败时抛出异常
            /// </summary>
            /// <seealso cref="http://help.aliyun.com/document_detail/28756.html">STS API 参考</seealso>
            /// <seealso cref="http://help.aliyun.com/document_detail/28794.html">STS SDK 参考</seealso>
            private (String accessKeyId, String accessKey, String stsToken) AssumeRoleOrThrow()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                this.timer?.Dispose();
            }
        }
    }
}
