using com.etsoo.SMS;
using System;
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

        /// <summary>
        /// Unseal data
        /// 解封信息
        /// </summary>
        /// <param name="secureManager">Secure manager</param>
        public void UnsealData(Func<string, string, string> secureManager)
        {
            SMSUser = secureManager(nameof(SMSUser), SMSUser);
            SMSKey = secureManager(nameof(SMSKey), SMSKey);
        }
    }
}
