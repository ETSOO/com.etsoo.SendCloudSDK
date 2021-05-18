using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace com.etsoo.SendCloudSDK.Shared
{
    class DistinctItemComparer : IEqualityComparer<Country.Phone>
    {
        /// <summary>
        /// Equal check
        /// </summary>
        /// <param name="x">Object 1</param>
        /// <param name="y">Object 2</param>
        /// <returns>Result</returns>
        public bool Equals(Country.Phone? x, Country.Phone? y)
        {
            return x?.PhoneNumber == y?.PhoneNumber &&
                x?.IsMobile == y?.IsMobile &&
                x?.Country == y?.Country;
        }

        /// <summary>
        /// Get hash code
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Result</returns>
        public int GetHashCode(Country.Phone obj)
        {
            return obj.PhoneNumber.GetHashCode() ^ obj.IsMobile.GetHashCode() ^ obj.Country.GetHashCode();
        }
    }

    /// <summary>
    /// Extensions
    /// 扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Unique phones
        /// 唯一的电话号码
        /// </summary>
        /// <param name="phones">Phones</param>
        /// <returns>Result</returns>
        public static IEnumerable<Country.Phone> UniquePhones(this IEnumerable<Country.Phone> phones)
        {
            return phones.Distinct(new DistinctItemComparer());
        }

        /// <summary>
        /// Join as string, ended with itemSplitter
        /// 链接成字符串，以 itemSplitter 结尾
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Items</param>
        /// <param name="valueSplitter">Name / value splitter</param>
        /// <param name="itemSplitter">Item splitter</param>
        /// <returns></returns>
        public static string JoinAsString<T>(this IEnumerable<KeyValuePair<string, T>> items, string valueSplitter = "=", string itemSplitter = "&")
        {
            return items.Aggregate(new StringBuilder(), (s, x) => s.Append(x.Key + valueSplitter + x.Value + itemSplitter), s => s.ToString());
        }

        /// <summary>
        /// Join as web query, ended with itemSplitter
        /// 链接成网页查询字符串，以 itemSplitter 结尾
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Items</param>
        /// <param name="valueSplitter">Name / value splitter</param>
        /// <param name="itemSplitter">Item splitter</param>
        /// <returns>Result</returns>
        public static string JoinAsQuery<T>(this IEnumerable<KeyValuePair<string, T>> items, string valueSplitter = "=", string itemSplitter = "&")
        {
            return items.Aggregate(new StringBuilder(), (s, x) => s.Append(HttpUtility.UrlEncode(x.Key) + valueSplitter + (x.Value == null ? String.Empty : HttpUtility.UrlEncode(x.Value.ToString())) + itemSplitter), s => s.ToString());
        }

        /// <summary>
        /// Convert to MD5 X2 string
        /// 转化为MD5 X2字符串
        /// </summary>
        /// <param name="source">Source</param>
        /// <returns>Result</returns>
        public static async Task<string> ToMD5X2Async(this string source)
        {
            using var md5 = MD5.Create();

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(source));
            var bytes = await md5.ComputeHashAsync(ms);

            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
