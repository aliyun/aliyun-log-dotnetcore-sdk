//
// IndexLineInfo.cs
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
using Aliyun.Api.LogService.Utils;

namespace Aliyun.Api.LogService.Domain.LogStore.Index
{
    public class IndexLineInfo
    {
        /// <summary>
        /// 是否区分大小写，默认值为false，表示不区分。
        /// </summary>
        public Boolean? CaseSensitive { get; set; }

        /// <summary>
        /// 分词字符列表，只支持单个英文字符。
        /// </summary>
        public IEnumerable<Char> Token { get; }

        /// <summary>
        /// 是否进行中文分词，默认值为false，表示不进行中文分词。
        /// </summary>
        public Boolean? Chn { get; set; }

        public IndexLineInfo(IEnumerable<Char> token)
        {
            this.Token = token.Freeze();
        }
    }
}
