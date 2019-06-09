namespace ShaclService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Writing;

    public static class Configuration
    {
        public static IEnumerable<(string MediaType, string Extension, Action<IGraph, TextReader> Read, Action<IGraph, TextWriter> Write)> MediaTypes { get; } = new (string, string, Action<IGraph, TextReader>, Action<IGraph, TextWriter>)[]
        {
            ("text/html", "html", null, null),
            ("text/turtle", "ttl", (g, reader) => new TurtleParser().Load(g, reader), (g, writer) => new CompressingTurtleWriter().Save(g, writer)),
            ("text/n-triples", "nt", (g, reader) => new NTriplesParser().Load(g, reader), (g, writer) => new NTriplesWriter().Save(g, writer)),
            ("application/ld+json", "json", (g, reader) => new JsonLdParser().Load(g.AsTripleStore(), reader), (g, writer) => new JsonLdWriter().Save(g.AsTripleStore(), writer)),
            ("application/rdf+xml", "xml", (g, reader) => new RdfXmlParser().Load(g, reader), (g, writer) => new RdfXmlWriter(WriterCompressionLevel.High, false).Save(g, writer)),
            ("application/rdf+json", "rj", (g, reader) => new RdfJsonParser().Load(g, reader), (g, writer) => new RdfJsonWriter().Save(g, writer)),
        };
    }
}
