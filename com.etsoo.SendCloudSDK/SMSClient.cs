using com.etsoo.SendCloudSDK.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace com.etsoo.SendCloudSDK
{
    /// <summary>
    /// SMS action result
    /// 短信操作结果
    /// </summary>
    public record SMSActionResult(bool Result, int? StatusCode, string? Message = null, Dictionary<string, object>? Info = null);

    /// <summary>
    /// SMS Client
    /// https://www.sendcloud.net/doc/sms/api/
    /// 短信客户端
    /// </summary>
    public class SMSClient : TemplateClient, ISMSClient
    {
        private readonly HttpClient httpClient;
        private readonly string smsUser;
        private readonly string smsKey;

        /// <summary>
        /// Demestic country
        /// </summary>
        public Country Country { get; private set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="httpClient">Http client, use IHttpClientFactory to create, services.AddHttpClient</param>
        /// <param name="smsUser">SMS User</param>
        /// <param name="smsKey">SMS key</param>
        /// <param name="country">Demestic country</param>
        public SMSClient(HttpClient httpClient, string smsUser, string smsKey, Country country)
        {
            this.httpClient = httpClient;
            this.smsUser = smsUser;
            this.smsKey = smsKey;

            Country = country;
        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="httpClient">Http client, use IHttpClientFactory to create, services.AddHttpClient</param>
        /// <param name="smsUser">SMS User</param>
        /// <param name="smsKey">SMS key</param>
        public SMSClient(HttpClient httpClient, IConfigurationSection section) : this(
            httpClient, 
            section.GetValue<string>("SMSUser"), 
            section.GetValue<string>("SMSKey"),
            Countries.GetById(section.GetValue<string>("Country")) ?? Countries.CN
        )
        {
            var templates = section.GetSection("Templates").Get<IEnumerable<TemplateItem>>();
            AddTemplates(templates);
        }

        private async Task CreateSignatureAsync(SortedDictionary<string, string> data)
        {
            // Combine as string
            var source = smsKey + "&" + data.JoinAsString() + smsKey;

            // Calculate signature
            data["signature"] = await source.ToMD5X2Async();
        }

        /// <summary>
        /// Async send SMS with template id
        /// 异步通过模板编号发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="templateId">Template id</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<string> mobiles, Dictionary<string, string> vars, string templateId)
        {
            return await SendAsync(kind, Countries.CreatePhones(mobiles, Country.Id), vars, templateId);
        }

        /// <summary>
        /// Async send SMS with template id
        /// 异步通过模板编号发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="templateId">Template id</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<Country.Phone> mobiles, Dictionary<string, string> vars, string templateId)
        {
            return await SendAsync(kind, mobiles, vars, GetTemplate(TemplateKind.Code, templateId));
        }

        /// <summary>
        /// Async send SMS
        /// 异步发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="template">Template</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<string> mobiles, Dictionary<string, string> vars, TemplateItem? template = null)
        {
            return await SendAsync(kind, Countries.CreatePhones(mobiles, Country.Id), vars, template);
        }

        /// <summary>
        /// Async send SMS
        /// 异步发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="template">Template</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<Country.Phone> mobiles, Dictionary<string, string> vars, TemplateItem? template = null)
        {
            // Mobile only and avoid duplicate items
            var validatedMobiles = mobiles.UniquePhones().Where(m => m.IsMobile);

            var count = validatedMobiles.Count();
            if (count == 0)
            {
                return new ActionResult(false, -1, "No Valid Item");
            }
            else if (count > 2000)
            {
                return new ActionResult(false, -1, "Max 2000 Items");
            }

            // Is international
            bool intl;

            if (template == null)
            {
                // Countries
                var countries = validatedMobiles.GroupBy(m => m.Country).Select(g => g.Key);

                // If more then one country or different with the default country
                var countriesCount = countries.Count();
                var firstCountry = countries.First();
                intl = countriesCount > 1 || firstCountry != Country.Id;

                // Default template
                template = GetTemplate(kind, country: (countriesCount > 1 ? null : firstCountry));
                if (template == null)
                {
                    throw new ArgumentNullException(nameof(template));
                }
            }
            else if (template.Country == null)
            {
                // No country specified
                intl = mobiles.Any(m => m.Country != Country.Id);
            }
            else
            {
                // Specific country
                intl = template.Country != Country.Id;
            }

            // Is domestic
            var msgType = intl ? 2 : 0;

            // Join all numbers
            var numbers = validatedMobiles.Select(m => intl ? m.ToInternationalFormat(Country.ExitCode) : m.PhoneNumber);

            // Variables to JSON
            var varsJson = JsonSerializer.Serialize(vars, new JsonSerializerOptions { WriteIndented = false, AllowTrailingCommas = false });

            // Post data
            var data = new SortedDictionary<string, string>
            {
                ["smsUser"] = smsUser,
                ["templateId"] = template.TemplateId,
                ["phone"] = string.Join(',', numbers),
                ["msgType"] = msgType.ToString(),
                ["vars"] = varsJson
            };

            // Create signature
            await CreateSignatureAsync(data);

            // Endpoint
            var endPoint = template.EndPoint ?? "https://www.sendcloud.net/smsapi/send";

            // Post
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            var response = await httpClient.PostAsync(endPoint, new FormUrlEncodedContent(data));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

            // Result
            var result = await response.Content.ReadFromJsonAsync<SMSActionResult>();

            // Return
            return new ActionResult(result?.Result ?? false, result?.StatusCode, result?.Message, result?.Info);
        }

        /// <summary>
        /// Async send code with template id
        /// 异步通过模板编号发送验证码
        /// </summary>
        /// <param name="mobile">Mobile</param>
        /// <param name="code">Code</param>
        /// <param name="templateId">Template id</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendCodeAsync(string mobile, string code, string templateId)
        {
            return await SendCodeAsync(mobile, code, GetTemplate(TemplateKind.Code, templateId));
        }

        /// <summary>
        /// Async send code
        /// 异步发送验证码
        /// </summary>
        /// <param name="mobile">Mobile</param>
        /// <param name="code">Code</param>
        /// <param name="template">Template</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendCodeAsync(string mobile, string code, TemplateItem? template = null)
        {
            var vars = new Dictionary<string, string>
            {
                ["code"] = code
            };

            return await SendAsync(TemplateKind.Code, new List<string> { mobile }, vars, template);
        }
    }
}
