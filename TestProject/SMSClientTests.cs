using com.etsoo.SendCloudSDK;
using com.etsoo.SendCloudSDK.Shared;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestProject
{
    public class SMSClientTests
    {
        readonly ISMSClient client;

        public SMSClientTests()
        {
            client = new SMSClient(new HttpClient(), "etsoo", "*JfGcgHp4wB4JPTKG5CHnFPkVwX0hj15N", Countries.CN);
            client.AddTemplate(new TemplateItem(TemplateKind.Code, "762226", Country: "CN", Default: true));
            client.AddTemplate(new TemplateItem(TemplateKind.Code, "762227", Default: true));
        }

        [Test]
        public void SMSClient_ConfigurationInit_Tests()
        {
            // Arrange
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(@"{
                ""SMS"": {
                    ""SMSUser"": ""etsoo"",
                    ""SMSKey"": ""JfGcgHp4wB4JPTKG5CHnFPkVwX0hj15N"",
                    ""Country"": ""CN"",
                    ""Templates"": [
                        {
                            ""Kind"": ""Code"",
                            ""TemplateId"": ""762226"",
                            ""Country"": ""CN"",
                            ""Language"": ""zh-CN"",
                            ""Default"": true
                        },
                        {
                            ""Kind"": ""Code"",
                            ""TemplateId"": ""762227"",
                            ""Default"": true
                        }
                    ]
                }
            }"));
            var section = new ConfigurationBuilder().AddJsonStream(stream).Build().GetSection("SMS");

            // Act
            var client = new SMSClient(new HttpClient(), section);

            // Assert
            Assert.AreEqual("CN", client.Country.Id);
            Assert.AreEqual("CN", client.GetTemplate(TemplateKind.Code, "762226")?.Country);
        }

        [Test]
        public async Task SendCodeAsync_Tests()
        {
            // Arrange
            var mobile = Countries.CreatePhone("+64210722065");
            if (mobile == null)
            {
                Assert.Fail("Mobile phone number is invalid");
                return;
            }

            // Act
            var result = await client.SendCodeAsync(mobile, "123456");

            // Assert
            Assert.AreEqual(false, result.Success);
        }
    }
}