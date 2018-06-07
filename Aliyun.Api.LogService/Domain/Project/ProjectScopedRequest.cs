using System;
using Newtonsoft.Json;

namespace Aliyun.Api.LogService.Domain.Project
{
    /// <summary>
    /// 限定在指定Project内的请求，可带有 <see cref="ProjectName"/> 属性，覆盖 <see cref="ILogServiceClient"/> 中默认的 Project。 
    /// </summary>
    public abstract class ProjectScopedRequest
    {
        /// <summary>
        /// 覆盖 <see cref="ILogServiceClient"/> 中默认的 Project。 
        /// </summary>
        [JsonIgnore]
        public String ProjectName { get; set; }
    }
}
