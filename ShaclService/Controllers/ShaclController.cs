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
    public IActionResult Validate(Parameters parameters)
    {
        // TODO: Move to parameters
        if (parameters.DataGraphUri is null && parameters.ShapesGraphUri is null && parameters.DataGraphRdf is null && parameters.ShapesGraphRdf is null)
        {
            return View((parameters, (Report)null));
        }

        // TODO: Validation
        if (parameters.DataGraphUri is not null && !parameters.DataGraphUri.IsAbsoluteUri || parameters.ShapesGraphUri is not null && !parameters.ShapesGraphUri.IsAbsoluteUri)
        {
            return BadRequest("Absolute URIs only.");
        }

        return Validate(parameters.DataGraph, parameters.ShapesGraph, parameters);
    }

    [HttpPost("validate")]
    [HttpPost("validate.{format}")]
    [FormatFilter]
    public IActionResult Validate([FromBody] IGraph g, Parameters parameters)
    {
        // TODO: Validation
        if (g is null || g.IsEmpty)
        {
            return Validate(parameters);
        }

        return Validate(g, g, parameters);
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
            return View((parameters, (bool?)null));
        }

        // TODO: Validation
        if (parameters.DataGraphUri is not null && !parameters.DataGraphUri.IsAbsoluteUri || parameters.ShapesGraphUri is not null && !parameters.ShapesGraphUri.IsAbsoluteUri)
        {
            return BadRequest("Absolute URIs only.");
        }

        return Conforms(parameters.DataGraph, parameters.ShapesGraph, parameters);
    }

    [HttpPost("conforms")]
    [FormatFilter]
    public IActionResult Conforms([FromBody] IGraph g, Parameters parameters)
    {
        // TODO: Validation
        if (g is null || g.IsEmpty)
        {
            return Conforms(parameters);
        }

        return Conforms(g, g, parameters);
    }

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
                (s, e) => null,
                typeof(IGraph),
                null),
            options.OutputFormatters.OfType<GraphOutputFormatter>().Cast<IOutputFormatter>().ToList(),
            mediaTypes);

        return ((GraphOutputFormatter)formatter).SupportedMediaTypes.Single();
    }
}
