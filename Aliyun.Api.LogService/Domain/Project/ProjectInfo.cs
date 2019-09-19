//
// ProjectInfo.cs
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

namespace Aliyun.Api.LogService.Domain.Project
{
    public class ProjectInfo
    {
        /// <summary>
        /// project名称。
        /// </summary>
        public String ProjectName { get; set; }

        /// <summary>
        /// project状态，分为"Normal"（正常）和"Disable"（欠费停用）。
        /// </summary>
        public ProjectState Status { get; set; }

        /// <summary>
        /// project描述。
        /// </summary>
        public String Description { get; set; }

        public ProjectInfo(String projectName, ProjectState status, String description)
        {
            this.ProjectName = projectName;
            this.Status = status;
            this.Description = description;
        }

        public ProjectInfo()
        {

        }
    }
}
