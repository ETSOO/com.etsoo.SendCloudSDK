using com.etsoo.Address;
using com.etsoo.SMS;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Crypto;
using com.etsoo.Utils.String;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
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
        private readonly SMSClientOptions options;

        /// <summary>
        /// Demestic country or region
        /// </summary>
        public AddressRegion Region { get; private set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="options">Options</param>
        /// <param name="httpClient">Http client, use IHttpClientFactory to create, services.AddHttpClient</param>
        public SMSClient(SMSClientOptions options, HttpClient httpClient) : base(httpClient)
        {
            if (options.Templates?.Any() is true) AddTemplates(options.Templates);

            this.options = options;
            Region = AddressRegion.GetById(options.Region) ?? AddressRegion.CN;
        }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="options">Options</param>
        /// <param name="httpClient">HTTP client</param>
        [ActivatorUtilitiesConstructor]
        public SMSClient(IOptions<SMSClientOptions> options, HttpClient httpClient)
            : this(options.Value, httpClient)
        {

        }

        private async Task CreateSignatureAsync(SortedDictionary<string, string> data)
        {
            // Combine as string
            var smsKey = options.SMSKey;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<string> mobiles, Dictionary<string, string> vars, string templateId, CancellationToken cancellationToken = default)
        {
            return await SendAsync(kind, AddressRegion.CreatePhones(mobiles, Region.Id), vars, templateId, cancellationToken);
        }

        /// <summary>
        /// Async send SMS with template id
        /// 异步通过模板编号发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="templateId">Template id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<AddressRegion.Phone> mobiles, Dictionary<string, string> vars, string templateId, CancellationToken cancellationToken = default)
        {
            return await SendAsync(kind, mobiles, vars, GetTemplate(TemplateKind.Code, templateId), cancellationToken);
        }

        /// <summary>
        /// Async send SMS
        /// 异步发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="template">Template</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<string> mobiles, Dictionary<string, string> vars, TemplateItem? template = null, CancellationToken cancellationToken = default)
        {
            return await SendAsync(kind, AddressRegion.CreatePhones(mobiles, Region.Id), vars, template, cancellationToken);
        }

        /// <summary>
        /// Async send SMS
        /// 异步发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="template">Template</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<AddressRegion.Phone> mobiles, Dictionary<string, string> vars, TemplateItem? template = null, CancellationToken cancellationToken = default)
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
                ["smsUser"] = options.SMSUser,
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
            var result = await PostFormAsync<SMSActionResult>(endPoint, data, cancellationToken);

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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendCodeAsync(AddressRegion.Phone mobile, string code, string templateId, CancellationToken cancellationToken = default)
        {
            return await SendCodeAsync(mobile, code, GetTemplate(TemplateKind.Code, templateId), cancellationToken);
        }

        /// <summary>
        /// Async send code
        /// 异步发送验证码
        /// </summary>
        /// <param name="mobile">Mobile</param>
        /// <param name="code">Code</param>
        /// <param name="template">Template</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Result</returns>
        public async Task<ActionResult> SendCodeAsync(AddressRegion.Phone mobile, string code, TemplateItem? template = null, CancellationToken cancellationToken = default)
        {
            var vars = new Dictionary<string, string>
            {
                ["code"] = code
            };

            return await SendAsync(TemplateKind.Code, new List<AddressRegion.Phone> { mobile }, vars, template, cancellationToken);
        }
    }
}
