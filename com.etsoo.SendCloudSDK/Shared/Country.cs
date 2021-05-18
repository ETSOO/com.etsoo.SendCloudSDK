using System.Linq;

namespace com.etsoo.SendCloudSDK.Shared
{
    /// <summary>
    /// Country phone validator delegate
    /// </summary>
    /// <param name="phoneNumber">Phone numbers</param>
    /// <param name="isMobile">Is mobile</param>
    /// <param name="Idd">IDD</param>
    /// <returns>Result</returns>
    public delegate bool CountryPhoneValidator(ref string phoneNumber, ref bool? isMobile, string Idd);

    /// <summary>
    /// Country
    /// 国家
    /// </summary>
    public record Country (
        // CN
        string Id,
        
        // CHN
        string Id3,

        // 156
        string Nid,

        // AS
        string Continent,

        // 00
        string ExitCode,

        // 86
        string Idd,

        // CNY / 币种
        string Currency,

        // Primary language / 第一语言
        string Language,

        // Phone validator
        CountryPhoneValidator? PhoneValidator = null
    )
    {
        /// <summary>
        /// Phone number with country
        /// 带有国家标识的电话号码
        /// </summary>
        public record Phone
        {
            /// <summary>
            /// Phone number
            /// 电话号码
            /// </summary>
            public string PhoneNumber { get; init; }

            /// <summary>
            /// Is mobile
            /// 是否为移动号码
            /// </summary>
            public bool IsMobile { get; init; }

            /// <summary>
            /// Country
            /// 所在国家
            /// </summary>
            public string Country { get; init; }

            internal Phone(string phoneNumber, bool isMobile, string country)
            {
                PhoneNumber = phoneNumber;
                IsMobile = isMobile;
                Country = country;
            }

            /// <summary>
            /// Convert to international dialing format
            /// 转换为国际拨号格式
            /// </summary>
            /// <param name="exitCode">Exit code, default is standard +</param>
            /// <returns>Result</returns>
            public string ToInternationalFormat(string exitCode = "+")
            {
                return string.Concat(exitCode, Countries.GetById(Country)?.Idd, IsMobile ? PhoneNumber : PhoneNumber.TrimStart('0'));
            }
        }

        /// <summary>
        /// Format phone number
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="isMobile">Is mobile</param>
        /// <returns>Result</returns>
        public Phone? FormatPhone(string phoneNumber, bool? isMobile = null)
        {
            if (IsValid(ref phoneNumber, ref isMobile))
            {
                return new Phone(phoneNumber, isMobile.GetValueOrDefault(), Id);
            }

            return null;
        }

        /// <summary>
        /// Is a valid phone number
        /// 是否是有效的电话号码
        /// </summary>
        /// <param name="phoneNumber">Phone number, ref as formated result</param>
        /// <param name="isMobile">Is mobile, null to be decided</param>
        /// <param name="country">Country</param>
        /// <returns>Result</returns>
        public bool IsValid(ref string phoneNumber, ref bool? isMobile)
        {
            // Remove empties
            phoneNumber = phoneNumber.Trim();

            if (phoneNumber.Length < 7)
            {
                // All phone numbers should be longer than 7 characters
                return false;
            }

            // Remove all other characters
            phoneNumber = string.Concat(phoneNumber.Where(c => c == '+' || c is (>= '0' and <= '9')));

            if (phoneNumber.StartsWith('+') && !phoneNumber.StartsWith('+' + Idd))
            {
                // Invalid IDD
                return false;
            }

            if (PhoneValidator != null)
            {
                // Do the validation
                return PhoneValidator(ref phoneNumber, ref isMobile, Idd);
            }

            return true;
        }
    }
}
