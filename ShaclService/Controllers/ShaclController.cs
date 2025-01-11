using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using ShaclService.Formatters;
using ShaclService.Models;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Shacl;

namespace ShaclService.Controllers;

public class ShaclController(IOptions<MvcOptions> options, OutputFormatterSelector selector) : Controller
{
    private readonly MvcOptions options = options.Value;

    [HttpGet("validate")]
    [HttpGet("validate.{format}")]
    [HttpHead("validate")]
    [HttpHead("validate.{format}")]
    [FormatFilter]
    public IActionResult Validate(Parameters parameters) => parameters switch
    {
        // TODO: Move to parameters
        { DataGraphUri: null, ShapesGraphUri: null, DataGraphRdf: null, ShapesGraphRdf: null } =>
            View((parameters, null as Report)),

        // TODO: Validation
        { DataGraphUri.IsAbsoluteUri: false } or
        { ShapesGraphUri.IsAbsoluteUri: false } =>
            BadRequest("Absolute URIs only."),

        _ => Validate(parameters.DataGraph, parameters.ShapesGraph, parameters),
    };

    [HttpPost("validate")]
    [HttpPost("validate.{format}")]
    [FormatFilter]
    public IActionResult Validate([FromBody] IGraph g, Parameters parameters) => g switch
    {
        // TODO: Validation
        null or { IsEmpty: true } => Validate(parameters),

        _ => Validate(g, g, parameters),
    };

    [HttpGet("conforms")]
    [HttpGet("conforms.{format}")]
    [HttpPost("conforms.{format}")]
    [HttpHead("conforms")]
    [HttpHead("conforms.{format}")]
    [FormatFilter]
    public IActionResult Conforms(Parameters parameters) => parameters switch
    {
        // TODO: Move to parameters
        { DataGraphUri: null, ShapesGraphUri: null, DataGraphRdf: null, ShapesGraphRdf: null } =>
            View((parameters, null as bool?)),

        // TODO: Validation
        { DataGraphUri.IsAbsoluteUri: false } or
        { ShapesGraphUri.IsAbsoluteUri: false } =>
            BadRequest("Absolute URIs only."),

        _ => Conforms(parameters.DataGraph, parameters.ShapesGraph, parameters),
    };


    [HttpPost("conforms")]
    [FormatFilter]
    public IActionResult Conforms([FromBody] IGraph g, Parameters parameters) => g switch
    {
        // TODO: Validation
        null or { IsEmpty: true } => Conforms(parameters),

        _ => Conforms(g, g, parameters),
    };

    private IActionResult Validate(IGraph data, IGraph shapes, Parameters parameters)
    {
        var report = new ShapesGraph(shapes).Validate(data).Normalised;

        return GetResponseContentType(parameters.Format) switch
        {
            "text/html" => View((parameters, Report.Parse(report))),
            _ => Ok(report),
        };
    }

    private IActionResult Conforms(IGraph data, IGraph shapes, Parameters parameters)
    {
        var conforms = new ShapesGraph(shapes).Conforms(data);

        return GetResponseContentType(parameters.Format) switch
        {
            "text/html" => View((parameters, (bool?)conforms)),
            _ => Ok(conforms),
        };
    }

    private string GetResponseContentType(string format)
    {
        var mediaTypes = new MediaTypeCollection();
        if (format is not null)
        {
            mediaTypes.Add(
                new MediaTypeHeaderValue(
                    options.FormatterMappings.GetMediaTypeMappingForFormat(
                        format)));
        }

        var formatter = selector.SelectFormatter(
            new OutputFormatterWriteContext(
                HttpContext,
                (_, _) => null,
                typeof(IGraph),
                null),
            options.OutputFormatters.OfType<GraphOutputFormatter>().Cast<IOutputFormatter>().ToList(),
            mediaTypes);

        return ((GraphOutputFormatter)formatter).SupportedMediaTypes.Single();
    }
}
