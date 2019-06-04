namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using VDS.RDF;
    using VDS.RDF.Writing;

    internal class GraphOutputFormatter : TextOutputFormatter
    {
        private static readonly IDictionary<string, Action<IGraph, TextWriter>> parserMapping = new Dictionary<string, Action<IGraph, TextWriter>>
        {
            { "text/turtle", (g, writer) => new CompressingTurtleWriter().Save(g, writer) },
            { "text/n-triples", (g, writer) => new NTriplesWriter().Save(g, writer) },
            { "application/ld+json", (g, writer) => new JsonLdWriter().Save(g.AsTripleStore(), writer) },
            { "application/rdf+xml", (g, writer) => new RdfXmlWriter().Save(g, writer) },
            { "application/rdf+json", (g, writer) => new RdfJsonWriter().Save(g, writer) }
        };

        public GraphOutputFormatter()
        {
            foreach (var mapping in parserMapping)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mapping.Key));
            }

            SupportedEncodings.Add(new UTF8Encoding(false));
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IGraph).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding encoding)
        {
            var g = (IGraph)context.Object;
            var write = parserMapping[context.HttpContext.Request.Headers["Accept"].AsMediaType()];

            using (var writer = context.WriterFactory(context.HttpContext.Response.Body, encoding))
            {
                write(g, writer);
            }

            return Task.FromResult(g);
        }
    }
}