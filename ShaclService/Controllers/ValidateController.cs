namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Shacl;

    [Route("validate")]
    public class ValidateController : Controller
    {
        private readonly MvcOptions options;
        private readonly OutputFormatterSelector selector;

        public ValidateController(IOptions<MvcOptions> options, OutputFormatterSelector selector)
        {
            this.options = options.Value;
            this.selector = selector;
        }

        [Route("report")]
        [Route("report.{format}")]
        [HttpGet]
        [HttpPost]
        [FormatFilter]
        public IActionResult Default(Parameters parameters)
        {
            var report = Validate(parameters.DataGraph, parameters.ShapesGraph);

            switch (GetResponseContentType(parameters.Format))
            {
                case "text/html":
                    return this.View(Report.Parse(report));

                default:
                    return this.Ok(report);
            }
        }

        [HttpPost("body")]
        public IGraph ValidateBody([FromBody] IGraph g) =>
            Validate(g, g);

        private static IGraph Validate(IGraph data, IGraph shapes) =>
            new ShapesGraph(shapes).Validate(data).Normalised;

        private string GetResponseContentType(string format)
        {
            var mediaTypes = new MediaTypeCollection();
            if (format != null)
            {
                mediaTypes.Add(
                    new MediaTypeHeaderValue(
                        options.FormatterMappings.GetMediaTypeMappingForFormat(
                            format)));
            }

            var formatter = selector.SelectFormatter(
                new OutputFormatterWriteContext(
                    this.HttpContext,
                    (s, e) => null,
                    typeof(IGraph),
                    null),
                options.OutputFormatters.OfType<GraphOutputFormatter>().Cast<IOutputFormatter>().ToList(),
                mediaTypes);

            return ((GraphOutputFormatter)formatter).SupportedMediaTypes.Single();
        }
    }
}