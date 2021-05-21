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
            // Arrange & act
            var result = await client.SendCodeAsync("+64210722065", "123456");

            // Assert
            Assert.AreEqual(false, result.Success);
        }
    }
}