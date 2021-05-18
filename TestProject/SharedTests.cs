using com.etsoo.SendCloudSDK.Shared;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject
{
    public class SharedTests
    {
        [Test]
        public void Countries_GetById_Tests()
        {
            // Arrange & act
            var country = Countries.GetById("CN");

            // Assert
            Assert.IsTrue(country?.Currency == "CNY");
        }

        [Test]
        public void Countries_GetByIdd_Tests()
        {
            // Arrange & act
            var country = Countries.GetByIdd("64");

            // Assert
            Assert.IsTrue(country?.Id == "NZ");
        }

        private static IEnumerable<TestCaseData> CreatePhoneBulkTestData
        {
            get
            {
                yield return new TestCaseData("+8613832922812", "CN", "13832922812", true);
                yield return new TestCaseData("+8653255579200", "CN", "053255579200", false);
            }
        }

        [Test, TestCaseSource(nameof(CreatePhoneBulkTestData))]
        public void Countries_CreatePhone_BulkTests(string phoneNumber, string country, string formatedNumber, bool isMobile)
        {
            // Arrange & act
            var phone = Countries.CreatePhone(phoneNumber);

            Assert.AreEqual(country, phone?.Country);
            Assert.AreEqual(formatedNumber, phone?.PhoneNumber);
            Assert.AreEqual(isMobile, phone?.IsMobile);
        }

        [Test]
        public void Countries_CreatePhones_Test()
        {
            // Arrange
            var phoneNumbers = new List<string>
            {
                "+8613832922812", "+86532555792", "53255579200"
            };

            // Act
            var phones = Countries.CreatePhones(phoneNumbers, "CN");

            // Assert
            Assert.AreEqual(1, phones.Count());
        }

        [Test]
        public void CountryPhone_ToInternationalFormat_Tests()
        {
            // Arrange
            var phone = Countries.CreatePhone("0210722065", "NZ");

            // Act 1
            var result1 = phone?.ToInternationalFormat();

            // Assert 1
            Assert.AreEqual("+64210722065", result1);

            // Act 2
            var result2 = phone?.ToInternationalFormat("00");

            // Assert 2
            Assert.AreEqual("0064210722065", result2);
        }

        [Test]
        public void Extensions_UniquePhones_Tests()
        {
            // Arrange
            var phones = Countries.CreatePhones(new [] { "13853259135", "+64210722065", "+8613853259135" }, "CN");

            // Act & assert
            Assert.AreEqual(2, phones.UniquePhones().Count());
        }

        [Test]
        public void Extensions_JoinAsString_Tests()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                ["a"] = "1",
                ["b"] = "2"
            };

            // Act
            var result = data.JoinAsString(",", ";");

            // Assert
            Assert.AreEqual("a,1;b,2;", result);
        }

        [Test]
        public void Extensions_JoinAsQuery_Tests()
        {
            // Arrange
            var data = new Dictionary<string, string>
            {
                ["a"] = "1=2",
                ["b"] = "2&3"
            };

            // Act
            var result = data.JoinAsQuery();

            // Assert
            Assert.AreEqual("a=1%3d2&b=2%263&", result);
        }

        [Test]
        public async Task Extensions_ToMD5X2Async_Tests()
        {
            // Arrange & act
            var result = await "info@etsoo.com".ToMD5X2Async();

            // Assert
            Assert.AreEqual("9c7ce665e6f4f4c807912a7486244c90", result);
        }
    }
}