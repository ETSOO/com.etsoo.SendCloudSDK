using com.etsoo.SendCloudSDK;
using com.etsoo.SendCloudSDK.Shared;
using NUnit.Framework;
using System.Net.Http;
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