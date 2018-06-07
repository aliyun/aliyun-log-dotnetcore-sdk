using System;

namespace Aliyun.Api.LogService.Infrastructure.Authentication
{
    /// <summary>
    /// 身份验证凭据。
    /// </summary>
    public class Credential
    {
        public String AccessKeyId { get; }

        public String AccessKey { get; }

        public String StsToken { get; }

        public Credential(String accessKeyId, String accessKey, String stsToken = null)
        {
            this.AccessKeyId = accessKeyId;
            this.AccessKey = accessKey;
            this.StsToken = stsToken;
        }
    }
}
