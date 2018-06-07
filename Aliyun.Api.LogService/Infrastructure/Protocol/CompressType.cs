//
// CompressType.cs
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

namespace Aliyun.Api.LogService.Infrastructure.Protocol
{
    /// <summary>
    /// 数据压缩类型。
    /// </summary>
    public enum CompressType
    {
        /// <summary>
        /// 无压缩（默认）
        /// </summary>
        None,
        
        /// <summary>
        /// 使用标准LZ4压缩方式。
        /// </summary>
        /// <seealso cref="http://wikipedia.org/wiki/LZ4_(compression_algorithm)"/>
        Lz4,
        
        /// <summary>
        /// 使用Deflate（Zlib包装，RFC 1950）压缩方式。
        /// </summary>
        /// <seealso cref="http://www.ietf.org/rfc/rfc1950.txt"/>
        /// <seealso cref="http://www.ietf.org/rfc/rfc1951.txt"/>
        Deflate,
    }
}
