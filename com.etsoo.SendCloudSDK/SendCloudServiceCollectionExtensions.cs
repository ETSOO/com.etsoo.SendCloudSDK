using com.etsoo.SMS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace com.etsoo.SendCloudSDK
{
    public static class SendCloudServiceCollectionExtensions
    {
        /// <summary>
        /// Add SendCloud client
        /// 添加 SendCloud 客户端
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration section</param>
        /// <returns>Services</returns>
        public static IServiceCollection AddSendCloudClient(this IServiceCollection services, IConfigurationSection configuration)
        {
            services.AddSingleton<IValidateOptions<SMSClientOptions>, ValidateSMSClientOptions>();
            services.AddOptions<SMSClientOptions>().Bind(configuration).ValidateOnStart();
            services.AddSingleton<ISMSClient, SMSClient>();

            return services;
        }
    }
}
