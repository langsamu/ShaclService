namespace ShaclServiceTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Net.Http.Headers;
    using Microsoft.OpenApi.Readers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ShaclService;

    [TestClass]
    public class WebTests
    {
        private static WebApplicationFactory<Startup> factory;
        private static HttpClient client;

        public static IEnumerable<object[]> ExtensionMappings => Configuration.MediaTypes.Select(m => new[] { m.Extension, m.MediaType });

        public static IEnumerable<object[]> MediaTypes => Configuration.MediaTypes.Select(m => new[] { m.MediaType });

        public static IEnumerable<object[]> HeadRequests
        {
            get
            {
                yield return new[] { "/" };
                yield return new[] { "/validate" };
                yield return new[] { "/conforms" };
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            factory = new WebApplicationFactory<Startup>();
            factory.Server.AllowSynchronousIO = true;
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
            using (var response = await client.GetAsync("/openapi"))
            {
                var result = await response.Content.ReadAsStringAsync();

                StringAssert.Contains(result, "swagger-ui-bundle");
            }
        }

        [TestMethod]
        [DynamicData(nameof(ExtensionMappings))]
        public async Task Negotiates_extensions(string extension, string mediaType)
        {
            using (var response = await client.GetAsync($"/validate.{extension}?DataGraphRdf=%40prefix+ex%3A+%3Chttp%3A%2F%2Fexample.com%2F%3E+.%0D%0A%0D%0Aex%3As+ex%3Ap+ex%3Ao+.&ShapesGraphRdf=%40prefix+sh%3A+%3Chttp%3A%2F%2Fwww.w3.org%2Fns%2Fshacl%23%3E+.%0D%0A%40prefix+ex%3A+%3Chttp%3A%2F%2Fexample.com%2F%3E+.%0D%0A%0D%0A%5B%0D%0A++++sh%3AtargetNode+ex%3As+%3B%0D%0A++++sh%3Aproperty+ex%3Ashape%0D%0A%5D+.%0D%0A%0D%0Aex%3Ashape%0D%0A++++sh%3Apath+ex%3Ap+%3B%0D%0A++++sh%3Aclass+ex%3AC+."))
            {
                Assert.AreEqual(mediaType, response.Content.Headers.ContentType.MediaType);
            }
        }

        [TestMethod]
        [DynamicData(nameof(MediaTypes))]
        public async Task Negotiates_accept_headers(string mediaType)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "/validate?DataGraphRdf=%40prefix+ex%3A+%3Chttp%3A%2F%2Fexample.com%2F%3E+.%0D%0A%0D%0Aex%3As+ex%3Ap+ex%3Ao+.&ShapesGraphRdf=%40prefix+sh%3A+%3Chttp%3A%2F%2Fwww.w3.org%2Fns%2Fshacl%23%3E+.%0D%0A%40prefix+ex%3A+%3Chttp%3A%2F%2Fexample.com%2F%3E+.%0D%0A%0D%0A%5B%0D%0A++++sh%3AtargetNode+ex%3As+%3B%0D%0A++++sh%3Aproperty+ex%3Ashape%0D%0A%5D+.%0D%0A%0D%0Aex%3Ashape%0D%0A++++sh%3Apath+ex%3Ap+%3B%0D%0A++++sh%3Aclass+ex%3AC+."))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));

                using (var response = await client.SendAsync(request))
                {
                    Assert.AreEqual(mediaType, response.Content.Headers.ContentType.MediaType);
                }
            }
        }

        [TestMethod]
        public async Task Sends_CORS_headers()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Options, "/"))
            {
                request.Headers.Add(HeaderNames.Origin, "example.com");
                request.Headers.Add(HeaderNames.AccessControlRequestHeaders, HeaderNames.ContentType);
                request.Headers.Add(HeaderNames.AccessControlRequestMethod, HttpMethods.Post);

                using (var response = await client.SendAsync(request))
                {
                    var exists = response.Headers.TryGetValues(HeaderNames.AccessControlAllowOrigin, out var values);
                    Assert.IsTrue(exists);
                    Assert.IsTrue(values.Contains("*"));

                    exists = response.Headers.TryGetValues(HeaderNames.AccessControlAllowHeaders, out values);
                    Assert.IsTrue(exists);
                    Assert.IsTrue(values.Contains(HeaderNames.ContentType));

                    exists = response.Headers.TryGetValues(HeaderNames.AccessControlAllowMethods, out values);
                    Assert.IsTrue(exists);
                    Assert.IsTrue(values.Contains(HttpMethods.Post));
                }
            }
        }

        [TestMethod]
        [DynamicData(nameof(HeadRequests))]
        public async Task Handles_head_requests(string requestUri)
        {
            using (var message = new HttpRequestMessage(HttpMethod.Head, requestUri))
            {
                using (var response = await client.SendAsync(message))
                {
                    Assert.IsTrue(response.IsSuccessStatusCode);
                }
            }
        }
    }
}
