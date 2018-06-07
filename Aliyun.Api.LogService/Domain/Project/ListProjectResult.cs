//
// ListProjectResult.cs
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

namespace Aliyun.Api.LogService.Domain.Project
{
    public class ListProjectResult
    {
        /// <summary>
        /// 返回project数量。
        /// </summary>
        public Int32 Count { get; }

        /// <summary>
        /// project总数。
        /// </summary>
        public Int32 Total { get; }

        /// <summary>
        /// 返回的project属性列表。
        /// </summary>
        public IList<ProjectInfo> Projects { get; }

        public ListProjectResult(Int32 count, Int32 total, IList<ProjectInfo> projects)
        {
            this.Count = count;
            this.Total = total;
            this.Projects = projects;
        }
    }
}
