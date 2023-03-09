using com.etsoo.SMS;
using System.Collections.Generic;

namespace com.etsoo.SendCloudSDK
{
    /// <summary>
    /// SMS client options
    /// 短信客户端配置
    /// </summary>
    public record SMSClientOptions
    {
        public required string SMSUser { get; set; }
        public required string SMSKey { get; set; }
        public string Region { get; init; } = "CN";
        public IEnumerable<TemplateItem>? Templates { get; init; }
    }
}
