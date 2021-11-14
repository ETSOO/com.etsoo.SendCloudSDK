using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Address;
using com.etsoo.Utils.Crypto;
using com.etsoo.Utils.Net.SMS;
using com.etsoo.Utils.String;
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
    public record SMSActionResult(bool Result, int? StatusCode, string? Message = null, Dictionary<string, object?>? Info = null);

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
        /// Demestic country or region
        /// </summary>
        public AddressRegion Region { get; private set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="httpClient">Http client, use IHttpClientFactory to create, services.AddHttpClient</param>
        /// <param name="smsUser">SMS User</param>
        /// <param name="smsKey">SMS key</param>
        /// <param name="region">Demestic country or region</param>
        public SMSClient(HttpClient httpClient, string smsUser, string smsKey, AddressRegion region)
        {
            this.httpClient = httpClient;
            this.smsUser = smsUser;
            this.smsKey = smsKey;

            Region = region;
        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="httpClient">Http client, use IHttpClientFactory to create, services.AddHttpClient</param>
        /// <param name="smsUser">SMS User</param>
        /// <param name="smsKey">SMS key</param>
        /// <param name="secureManager">Secure manager</param>
        public SMSClient(HttpClient httpClient, IConfigurationSection section, Func<string, string>? secureManager = null) : this(
            httpClient,
            CryptographyUtils.UnsealData(section.GetValue<string>("SMSUser"), secureManager),
            CryptographyUtils.UnsealData(section.GetValue<string>("SMSKey"), secureManager),
            AddressRegion.GetById(section.GetValue<string>("Region")) ?? AddressRegion.CN
        )
        {
            // var templates = section.GetSection("Templates").Get<TemplateItem[]>();
            var templates = section.GetSection("Templates").GetChildren().Select(item => new TemplateItem(
                    Enum.Parse<TemplateKind>(item.GetValue<string>("Kind")),
                    item.GetValue<string>("TemplateId"),
                    item.GetValue<string>("EndPoint"),
                    item.GetValue<string>("Region"),
                    item.GetValue<string>("Language"),
                    item.GetValue<string>("Signature"),
                    item.GetValue("Default", false)
                ));
            AddTemplates(templates);
        }

        private async Task CreateSignatureAsync(SortedDictionary<string, string> data)
        {
            // Combine as string
            var source = smsKey + "&" + data.JoinAsString() + smsKey;

            // Calculate signature
            var signatureBytes = await CryptographyUtils.MD5Async(source);
            data["signature"] = Convert.ToHexString(signatureBytes).ToLower();
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
            return await SendAsync(kind, AddressRegion.CreatePhones(mobiles, Region.Id), vars, templateId);
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
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<AddressRegion.Phone> mobiles, Dictionary<string, string> vars, string templateId)
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
            return await SendAsync(kind, AddressRegion.CreatePhones(mobiles, Region.Id), vars, template);
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
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<AddressRegion.Phone> mobiles, Dictionary<string, string> vars, TemplateItem? template = null)
        {
            // Mobile only and avoid duplicate items
            var validatedMobiles = mobiles.UniquePhones().Where(m => m.IsMobile);

            var count = validatedMobiles.Count();
            if (count == 0)
            {
                return new ActionResult { Status = -1, Title = "No Valid Item" };
            }
            else if (count > 2000)
            {
                return new ActionResult { Status = -1, Title = "Max 2000 Items" };
            }

            // Is international
            bool intl;

            if (template == null)
            {
                // Countries
                var countries = validatedMobiles.GroupBy(m => m.Region).Select(g => g.Key);

                // If more then one country or different with the default country
                var countriesCount = countries.Count();
                var firstCountry = countries.First();
                intl = countriesCount > 1 || firstCountry != Region.Id;

                // Default template
                template = GetTemplate(kind, region: (countriesCount > 1 ? null : firstCountry));
                if (template == null)
                {
                    throw new ArgumentNullException(nameof(template));
                }
            }
            else if (template.Region == null)
            {
                // No country specified
                intl = mobiles.Any(m => m.Region != Region.Id);
            }
            else
            {
                // Specific country
                intl = template.Region != Region.Id;
            }

            // Is domestic
            var msgType = intl ? 2 : 0;

            // Join all numbers
            var numbers = validatedMobiles.Select(m => intl ? m.ToInternationalFormat(Region.ExitCode) : m.PhoneNumber);

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

            // Data
            var info = result?.Info;
            var resultData = info == null ? new StringKeyDictionaryObject() : new StringKeyDictionaryObject(info);

            // Return
            return new ActionResult { Ok = result?.Result ?? false, Status = result?.StatusCode, Title = result?.Message, Data = resultData };
        }

        /// <summary>
        /// Async send code with template id
        /// 异步通过模板编号发送验证码
        /// </summary>
        /// <param name="mobile">Mobile</param>
        /// <param name="code">Code</param>
        /// <param name="templateId">Template id</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendCodeAsync(AddressRegion.Phone mobile, string code, string templateId)
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
        public async Task<ActionResult> SendCodeAsync(AddressRegion.Phone mobile, string code, TemplateItem? template = null)
        {
            var vars = new Dictionary<string, string>
            {
                ["code"] = code
            };

            return await SendAsync(TemplateKind.Code, new List<AddressRegion.Phone> { mobile }, vars, template);
        }
    }
}
