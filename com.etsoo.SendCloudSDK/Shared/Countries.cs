using System.Collections.Generic;
using System.Linq;

namespace com.etsoo.SendCloudSDK.Shared
{
    /// <summary>
    /// Countries
    /// https://www.iban.com/country-codes
    /// https://www.howtocallabroad.com/codes.html
    /// 所有国家
    /// </summary>
    public static class Countries
    {
        /// <summary>
        /// CN - China
        /// 中国
        /// </summary>
        public static Country CN => new("CN", "CHN", "156", "AS", "00", "86", "CNY", "zh-CN", (ref string phoneNumber, ref bool? isMobile, string Idd) =>
        {
            // International format
            var intl = phoneNumber.StartsWith('+');

            if (intl)
            {
                // Remove IDD
                phoneNumber = phoneNumber[(Idd.Length + 1)..];
            }

            var isMobileActual = phoneNumber.StartsWith("13") || phoneNumber.StartsWith("15");

            if (isMobile == null)
            {
                isMobile = isMobileActual;
            }
            else if (isMobile != isMobileActual)
            {
                // Required is not the same with actual
                return false;
            }

            if (intl && !isMobile.GetValueOrDefault())
            {
                // Add zero
                phoneNumber = '0' + phoneNumber;
            }

            if (isMobileActual)
            {
                return phoneNumber.Length == 11;
            }
            else
            {
                if (!phoneNumber.StartsWith('0'))
                    return false;

                return phoneNumber.Length is >= 11 and <= 12;
            }
        });

        /// <summary>
        /// HK - HK, China
        /// 中国香港
        /// </summary>
        public static Country HK => new("HK", "HKG", "344", "AS", "001", "852", "HKD", "zh-HK");

        /// <summary>
        /// SG - Singapore
        /// 新加坡
        /// </summary>
        public static Country SG => new("SG", "SGP", "702", "AS", "000", "65", "SGD", "zh-sg");

        /// <summary>
        /// JP - Japan
        /// 日本
        /// </summary>
        public static Country JP => new("JP", "JPN", "392", "AS", "010", "81", "JPY", "ja-JP");

        /// <summary>
        /// US - United States
        /// 美国
        /// </summary>
        public static Country US => new("US", "USA", "840", "NA", "011", "1", "USD", "en-US");

        /// <summary>
        /// CA - Canada
        /// 加拿大
        /// </summary>
        public static Country CA => new("CA", "CAN", "124", "NA", "011", "1", "USD", "en-US");

        /// <summary>
        /// AU - Australia
        /// 澳大利亚
        /// </summary>
        public static Country AU => new("AU", "AUS", "036", "OC", "0011", "61", "AUD", "en-AU");

        /// <summary>
        /// NZ - New Zealand
        /// 新西兰
        /// </summary>
        public static Country NZ => new("NZ", "NZL", "554", "OC", "00", "64", "NZD", "en-NZ", (ref string phoneNumber, ref bool? isMobile, string Idd) =>
        {
            // https://www.tnzi.com/numbering-plan
            // International format
            var intl = phoneNumber.StartsWith('+');

            if (intl)
            {
                // Remove IDD
                phoneNumber = phoneNumber[(Idd.Length + 1)..];

                // Add zero
                phoneNumber = '0' + phoneNumber;
            }

            var isMobileActual = phoneNumber.StartsWith("02") && !phoneNumber.StartsWith("0240");

            if (isMobile == null)
            {
                isMobile = isMobileActual;
            }
            else if (isMobile != isMobileActual)
            {
                // Required is not the same with actual
                return false;
            }

            return phoneNumber.Length is >= 9 and <= 11;
        });

        /// <summary>
        /// GB - Great Britain
        /// 英国
        /// </summary>
        public static Country GB => new("GB", "GBR", "826", "EU", "00", "44", "GBP", "en-GB");

        /// <summary>
        /// IE - Ireland
        /// 爱尔兰
        /// </summary>
        public static Country IE => new("IE", "IRL", "372", "EU", "00", "353", "IEP", "en-IE");

        /// <summary>
        /// DE - Germany
        /// 德国
        /// </summary>
        public static Country DE => new("DE", "DEU", "276", "EU", "00", "49", "EUR", "de-DE");

        /// <summary>
        /// FR - France
        /// 法国
        /// </summary>
        public static Country FR => new("FR", "FRA", "250", "EU", "00", "33", "EUR", "fr-FR");

        /// <summary>
        /// All countries
        /// 所有国家
        /// </summary>
        public static IEnumerable<Country> All => new List<Country>
        {
            CN,
            HK,
            SG,
            JP,
            US,
            CA,
            AU,
            NZ,
            GB,
            IE,
            DE,
            FR
        };

        /// <summary>
        /// Get country by id
        /// 从编号获取国家
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Result</returns>
        public static Country? GetById(string id)
        {
            return id switch
            {
                nameof(CN) => CN,
                nameof(HK) => HK,
                nameof(SG) => SG,
                nameof(JP) => JP,
                nameof(US) => US,
                nameof(CA) => CA,
                nameof(AU) => AU,
                nameof(NZ) => NZ,
                nameof(GB) => GB,
                nameof(IE) => IE,
                nameof(DE) => DE,
                nameof(FR) => FR,
                _ => null
            };
        }

        /// <summary>
        /// Get country by IDD
        /// 从国家拨号获取国家
        /// </summary>
        /// <param name="idd">IDD</param>
        /// <returns>Result</returns>
        public static Country? GetByIdd(string idd)
        {
            return All.FirstOrDefault(c => c.Idd == idd);
        }

        /// <summary>
        /// Create phone
        /// 创建电话对象
        /// </summary>
        /// <param name="phoneNumber">Phone number</param>
        /// <param name="countryId">Default country id</param>
        /// <returns>Result</returns>
        public static Country.Phone? CreatePhone(string phoneNumber, string? countryId = null)
        {
            // Remove empties
            phoneNumber = phoneNumber.Trim();

            Country? country;
            if (countryId == null)
            {
                // No country specified
                // Should start with +
                if (!phoneNumber.StartsWith('+'))
                {
                    return null;
                }

                country = All.FirstOrDefault(c => phoneNumber.StartsWith("+" + c.Idd));
            }
            else if (phoneNumber.StartsWith('+'))
            {
                // Specify country
                country = All.FirstOrDefault(c => phoneNumber.StartsWith("+" + c.Idd));
            }
            else
            {
                // Default country
                country = GetById(countryId);
            }

            if (country == null)
            {
                return null;
            }

            // Format the phone
            return country.FormatPhone(phoneNumber);
        }

        /// <summary>
        /// Create phones
        /// 创建多个电话对象
        /// </summary>
        /// <param name="phoneNumbers">Phone numbers</param>
        /// <param name="countryId">Default country id</param>
        /// <returns>Result</returns>
        public static IEnumerable<Country.Phone> CreatePhones(IEnumerable<string> phoneNumbers, string? countryId = null)
        {
            foreach (var phoneNumber in phoneNumbers)
            {
                var phone = CreatePhone(phoneNumber, countryId);
                if (phone == null)
                    yield break;

                yield return phone;
            }
        }
    }
}
