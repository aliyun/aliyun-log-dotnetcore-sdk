using System;
using System.Collections.Specialized;
using System.Web;

namespace Aliyun.Api.LogService
{
    public static class UriExtensions
    {
        public static NameValueCollection ParseQueryString(this Uri uri)
        {
            return HttpUtility.ParseQueryString(uri.Query);
        }
    }
}