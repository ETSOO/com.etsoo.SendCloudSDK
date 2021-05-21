using com.etsoo.SendCloudSDK.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.etsoo.SendCloudSDK
{
    /// <summary>
    /// SMS client interface
    /// 短信客户端接口
    /// </summary>
    public interface ISMSClient : ITemplateClient
    {
        /// <summary>
        /// Demestic country
        /// </summary>
        Country Country { get; }

        /// <summary>
        /// Async send SMS with template id
        /// 异步通过模板编号发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="templateId">Template id</param>
        /// <returns>Result</returns>
        Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<string> mobiles, Dictionary<string, string> vars, string templateId);

        /// <summary>
        /// Async send SMS with template id
        /// 异步通过模板编号发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="templateId">Template id</param>
        /// <returns>Result</returns>
        Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<Country.Phone> mobiles, Dictionary<string, string> vars, string templateId);

        /// <summary>
        /// Async send SMS
        /// 异步发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="template">Template</param>
        /// <returns>Result</returns>
        Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<string> mobiles, Dictionary<string, string> vars, TemplateItem? template = null);

        /// <summary>
        /// Async send SMS
        /// 异步发送短信
        /// </summary>
        /// <param name="kind">Template kind</param>
        /// <param name="mobiles">Mobiles</param>
        /// <param name="vars">Variables</param>
        /// <param name="template">Template</param>
        /// <returns>Result</returns>
        Task<ActionResult> SendAsync(TemplateKind kind, IEnumerable<Country.Phone> mobiles, Dictionary<string, string> vars, TemplateItem? template = null);

        /// <summary>
        /// Async send code with template id
        /// 异步通过模板编号发送验证码
        /// </summary>
        /// <param name="mobile">Mobile</param>
        /// <param name="code">Code</param>
        /// <param name="templateId">Template id</param>
        /// <returns>Result</returns>
        Task<ActionResult> SendCodeAsync(string mobile, string code, string templateId);

        /// <summary>
        /// Async send code
        /// 异步发送验证码
        /// </summary>
        /// <param name="mobile">Mobile</param>
        /// <param name="code">Code</param>
        /// <param name="template">Template</param>
        /// <returns>Result</returns>
        Task<ActionResult> SendCodeAsync(string mobile, string code, TemplateItem? template = null);
    }
}
