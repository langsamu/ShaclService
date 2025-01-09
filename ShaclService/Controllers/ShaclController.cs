using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Shacl;

namespace ShaclService;

public class ShaclController(IOptions<MvcOptions> options, OutputFormatterSelector selector) : Controller
{
    private readonly MvcOptions options = options.Value;

    [HttpGet("validate")]
    [HttpGet("validate.{format}")]
    [HttpHead("validate")]
    [HttpHead("validate.{format}")]
    [FormatFilter]
    public IActionResult Validate(Parameters parameters)
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

    [HttpPost("validate")]
    [HttpPost("validate.{format}")]
    [FormatFilter]
    public IActionResult Validate([FromBody] IGraph g, Parameters parameters)
    {
        // TODO: Validation
        if (g is null || g.IsEmpty)
        {
            return this.Validate(parameters);
        }

        return this.Validate(g, g, parameters);
    }

    [HttpGet("conforms")]
    [HttpGet("conforms.{format}")]
    [HttpPost("conforms.{format}")]
    [HttpHead("conforms")]
    [HttpHead("conforms.{format}")]
    [FormatFilter]
    public IActionResult Conforms(Parameters parameters)
    {
        // TODO: Move to parameters
        if (parameters.DataGraphUri is null && parameters.ShapesGraphUri is null && parameters.DataGraphRdf is null && parameters.ShapesGraphRdf is null)
        {
            return this.View((parameters, (bool?)null));
        }

        // TODO: Validation
        if ((parameters.DataGraphUri != null && !parameters.DataGraphUri.IsAbsoluteUri) || (parameters.ShapesGraphUri != null && !parameters.ShapesGraphUri.IsAbsoluteUri))
        {
            return this.BadRequest("Absolute URIs only.");
        }

        return this.Conforms(parameters.DataGraph, parameters.ShapesGraph, parameters);
    }

    [HttpPost("conforms")]
    [FormatFilter]
    public IActionResult Conforms([FromBody] IGraph g, Parameters parameters)
    {
        // TODO: Validation
        if (g is null || g.IsEmpty)
        {
            return this.Conforms(parameters);
        }

        return this.Conforms(g, g, parameters);
    }

    private IActionResult Validate(IGraph data, IGraph shapes, Parameters parameters)
    {
        var report = new ShapesGraph(shapes).Validate(data).Normalised;

        return this.GetResponseContentType(parameters.Format) switch
        {
            "text/html" => this.View((parameters, Report.Parse(report))),
            _ => this.Ok(report),
        };
    }

    private IActionResult Conforms(IGraph data, IGraph shapes, Parameters parameters)
    {
        var conforms = new ShapesGraph(shapes).Conforms(data);

        return this.GetResponseContentType(parameters.Format) switch
        {
            "text/html" => this.View((parameters, (bool?)conforms)),
            _ => this.Ok(conforms),
        };
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

        var formatter = selector.SelectFormatter(
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