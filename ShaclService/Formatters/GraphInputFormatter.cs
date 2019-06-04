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
    using VDS.RDF.Parsing;

    internal class GraphInputFormatter : TextInputFormatter
    {
        private static readonly IDictionary<string, Action<IGraph, TextReader>> parserMapping = new Dictionary<string, Action<IGraph, TextReader>>
        {
            { "text/turtle", (g, reader) => new TurtleParser().Load(g, reader) },
            { "text/n-triples", (g, reader) => new NTriplesParser().Load(g, reader) },
            { "application/ld+json", (g, reader) => new JsonLdParser().Load(g.AsTripleStore(), reader) },
            { "application/rdf+xml", (g, reader) => new RdfXmlParser().Load(g, reader) },
            { "application/rdf+json", (g, reader) => new RdfJsonParser().Load(g, reader) }
        };

        public GraphInputFormatter()
        {
            foreach (var mapping in parserMapping)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(mapping.Key));
            }

            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
        }

        protected override bool CanReadType(Type type)
        {
            return typeof(IGraph).IsAssignableFrom(type);
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var g = new Graph();
            var read = parserMapping[context.HttpContext.Request.Headers["Content-Type"].AsMediaType()];

            using (var reader = new StreamReader(context.HttpContext.Request.Body, encoding))
            {
                read(g, reader);
            }

            return InputFormatterResult.SuccessAsync(g);
        }
    }
}