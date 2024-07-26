using com.etsoo.SMS;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace com.etsoo.SendCloudSDK
{
    /// <summary>
    /// SMS client options
    /// 短信客户端配置
    /// </summary>
    public record SMSClientOptions
    {
        [Required]
        public string SMSUser { get; set; } = string.Empty;

        [Required]
        public required string SMSKey { get; set; } = string.Empty;

        public string Region { get; set; } = "CN";

        public IEnumerable<TemplateItem>? Templates { get; set; }
    }

    [OptionsValidator]
    public partial class ValidateSMSClientOptions : IValidateOptions<SMSClientOptions>
    {
    }
}
