using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace ShaclService.Formatters;

internal class GraphInputFormatter : TextInputFormatter
{
    private readonly Action<IGraph, TextReader> read;

    public GraphInputFormatter(string mediaType, Action<IGraph, TextReader> read)
    {
        this.read = read;
        SupportedMediaTypes.Add(new(mediaType));
        SupportedEncodings.Add(UTF8EncodingWithoutBOM);
    }

    public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        var g = new Graph();

        using (var reader = context.ReaderFactory(context.HttpContext.Request.Body, encoding))
        {
            read(g, reader);
        }

        return InputFormatterResult.SuccessAsync(g);
    }

    protected override bool CanReadType(Type type) => typeof(IGraph).IsAssignableFrom(type);
}
