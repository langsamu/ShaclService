namespace ShaclService
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using VDS.RDF;
    using VDS.RDF.Shacl;

    [Route("validate")]
    [Route("validate.{format}")]
    public class ValidateController : Controller
    {
        private readonly MvcOptions options;
        private readonly OutputFormatterSelector selector;

        public ValidateController(IOptions<MvcOptions> options, OutputFormatterSelector selector)
        {
            this.options = options.Value;
            this.selector = selector;
        }

        [HttpGet]
        [FormatFilter]
        public IActionResult Index(Parameters parameters)
        {
            // TODO: Move to parameters
            if (parameters.DataGraphUri is null && parameters.ShapesGraphUri is null && parameters.DataGraphRdf is null && parameters.ShapesGraphRdf is null)
            {
                return this.View((parameters, (Report)null));
            }

            // TODO: Validation
            if ((parameters.DataGraphUri != null && !parameters.DataGraphUri.IsAbsoluteUri) || (parameters.ShapesGraphUri != null && !parameters.ShapesGraphUri.IsAbsoluteUri))
            {
                return this.BadRequest("Absolute URIs only.");
            }

            return this.Validate(parameters.DataGraph, parameters.ShapesGraph, parameters);
        }

        [HttpPost]
        [FormatFilter]
        public IActionResult Index([FromBody] IGraph g, Parameters parameters)
        {
            if (g is null || g.IsEmpty)
            {
                return this.Index(parameters);
            }

            return this.Validate(g, g, parameters);
        }

        private IActionResult Validate(IGraph data, IGraph shapes, Parameters parameters)
        {
            var report = new ShapesGraph(shapes).Validate(data).Normalised;

            switch (this.GetResponseContentType(parameters.Format))
            {
                case "text/html":
                    return this.View((parameters, Report.Parse(report)));

                default:
                    return this.Ok(report);
            }
        }

        private string GetResponseContentType(string format)
        {
            var mediaTypes = new MediaTypeCollection();
            if (format != null)
            {
                mediaTypes.Add(
                    new MediaTypeHeaderValue(
                        this.options.FormatterMappings.GetMediaTypeMappingForFormat(
                            format)));
            }

            var formatter = this.selector.SelectFormatter(
                new OutputFormatterWriteContext(
                    this.HttpContext,
                    (s, e) => null,
                    typeof(IGraph),
                    null),
                this.options.OutputFormatters.OfType<GraphOutputFormatter>().Cast<IOutputFormatter>().ToList(),
                mediaTypes);

            return ((GraphOutputFormatter)formatter).SupportedMediaTypes.Single();
        }
    }
}