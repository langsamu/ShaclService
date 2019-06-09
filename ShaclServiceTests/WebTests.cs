namespace ShaclServiceTests
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.OpenApi.Readers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ShaclService;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    [TestClass]
    public class WebTests
    {
        private static WebApplicationFactory<Startup> factory;
        private static HttpClient client;

        public static IEnumerable<object[]> ExtensionMappings => Configuration.MediaTypes.Select(m => new[] { m.Extension, m.MediaType });

        public static IEnumerable<object[]> MediaTypes => Configuration.MediaTypes.Select(m => new[] { m.MediaType });

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            factory = new WebApplicationFactory<Startup>();
            client = factory.CreateClient();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            client.Dispose();
            factory.Dispose();
        }

        [TestMethod]
        public async Task OpenApi_document_is_valid()
        {
            using (var response = await client.GetAsync($"/openapi.json"))
            {
                var reader = new OpenApiStreamReader();

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    reader.Read(stream, out var diagnostic);

                    Assert.IsFalse(diagnostic.Errors.Any(), string.Join(",", diagnostic.Errors));
                }
            }
        }

        [TestMethod]
        public async Task SwaggerUI_works()
        {
            using (var response = await client.GetAsync("/openapi.html"))
            {
                var result = await response.Content.ReadAsStringAsync();

                StringAssert.Contains(result, "SwaggerUIBundle");
            }
        }

        [TestMethod]
        [DynamicData(nameof(ExtensionMappings))]
        public async Task Negotiates_extensions(string extension, string mediaType)
        {
            using (var response = await client.GetAsync($"/validate/report.{extension}"))
            {
                Assert.AreEqual(mediaType, response.Content.Headers.ContentType.MediaType);
            }
        }

        [TestMethod]
        [DynamicData(nameof(MediaTypes))]
        public async Task Negotiates_accept_headers(string mediaType)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "/validate/report"))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

                using (var response = await client.SendAsync(request))
                {
                    Assert.AreEqual(mediaType, response.Content.Headers.ContentType.MediaType);
                }
            }
        }
    }
}
