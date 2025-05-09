﻿using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace ShaclService.Formatters;

internal class GraphOutputFormatter : TextOutputFormatter
{
    private readonly Action<IGraph, TextWriter> write;

    public GraphOutputFormatter(string mediaType, Action<IGraph, TextWriter> write)
    {
        this.write = write;
        SupportedMediaTypes.Add(new(mediaType));
        SupportedEncodings.Add(new UTF8Encoding(false));
    }

    public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
    {
        var g = (IGraph)context.Object;

        using (var writer = context.WriterFactory(context.HttpContext.Response.Body, encoding))
        {
            write(g, writer);
        }

        return Task.CompletedTask;
    }

    protected override bool CanWriteType(Type type) => typeof(IGraph).IsAssignableFrom(type);
}
