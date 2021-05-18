using System.Collections.Generic;

namespace com.etsoo.SendCloudSDK.Shared
{
    /// <summary>
    /// Action result
    /// 操作结果
    /// </summary>
    public record ActionResult (bool Success, int? StatusCode, string? Message = null, Dictionary<string, object>? Info = null);
}
