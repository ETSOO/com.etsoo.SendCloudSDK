using com.etsoo.Address;
using com.etsoo.SendCloudSDK;
using com.etsoo.SMS;
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

        // find the UserSecretId we added in the csproj file
        readonly IConfigurationRoot builder = new ConfigurationBuilder().AddUserSecrets<SMSClientTests>().Build();

        public SMSClientTests()
        {
            var smsUser = builder["SMSUser"];
            var smsPassword = builder["SMSPassword"];

            client = new SMSClient(new HttpClient(), smsUser, smsPassword, AddressRegion.CN);
            client.AddTemplate(new TemplateItem(TemplateKind.Code, "762226", Region: "CN", Default: true));
            client.AddTemplate(new TemplateItem(TemplateKind.Code, "762227", Default: true));
        }

        [Test]
        public void SMSClient_ConfigurationInit_Tests()
        {
            var smsUser = builder["SMSUser"];
            var smsPassword = builder["SMSPassword"];

            // Arrange
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(@$"{{
                ""SMS"": {{
                    ""SMSUser"": ""{smsUser}"",
                    ""SMSKey"": ""{smsPassword}"",
                    ""Region"": ""CN"",
                    ""Templates"": [
                        {{
                            ""Kind"": ""Code"",
                            ""TemplateId"": ""762226"",
                            ""Region"": ""CN"",
                            ""Language"": ""zh-CN"",
                            ""Default"": true
                        }},
                        {{
                            ""Kind"": ""Code"",
                            ""TemplateId"": ""762227"",
                            ""Default"": true
                        }}
                    ]
                }}
            }}"));
            var section = new ConfigurationBuilder().AddJsonStream(stream).Build().GetSection("SMS");

            // Act
            var client = new SMSClient(new HttpClient(), section);

            // Assert
            Assert.AreEqual("CN", client.Region.Id);
            Assert.AreEqual("CN", client.GetTemplate(TemplateKind.Code, "762226")?.Region);
        }

        [Test]
        public async Task SendCodeAsync_Tests()
        {
            // Arrange
            var mobile = AddressRegion.CreatePhone("+64210733065");
            if (mobile == null)
            {
                Assert.Fail("Mobile phone number is invalid");
                return;
            }

            // Act
            var result = await client.SendCodeAsync(mobile, "123456");

            // Assert
            Assert.AreEqual(true, result.Ok);
        }
    }
}