namespace com.etsoo.SendCloudSDK.Shared
{
    /// <summary>
    /// Template kind
    /// 模板类型
    /// </summary>
    public enum TemplateKind
    {
        /// <summary>
        /// Code
        /// 验证码
        /// </summary>
        Code,

        /// <summary>
        /// Notice
        /// 通知
        /// </summary>
        Notice,

        /// <summary>
        /// Marketing
        /// 营销
        /// </summary>
        Marketing
    }

    /// <summary>
    /// Template item definition
    /// 模板项目定义
    /// </summary>
    public record TemplateItem (TemplateKind Kind, string TemplateId, string? EndPoint = null, string? Country = null, string? Language = null, string? Signature = null, bool Default = false);
}
