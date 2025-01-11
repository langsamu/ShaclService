using System;
using System.Collections.Generic;
using System.IO;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace ShaclService.Models;

public static class Configuration
{
    public static IEnumerable<(string MediaType, string Extension, Action<IGraph, TextReader> Read, Action<IGraph, TextWriter> Write)> MediaTypes { get; } =
    [
        ("text/html", "html", null, null),
        ("text/turtle", "ttl", ReadTurtle, WriteTurtle),
        ("text/n-triples", "nt", ReadNt, WriteNt),
        ("application/ld+json", "json", ReadJson, WriteJson),
        ("application/rdf+xml", "xml", ReadXml, WriteXml),
        ("application/rdf+json", "rj", ReadRj, WriteRj),
        ("text/csv", "csv", null, WriteCsv),
    ];

    private static void ReadTurtle(IGraph g, TextReader reader) => new TurtleParser().Load(g, reader);
    private static void ReadNt(IGraph g, TextReader reader) => new NTriplesParser().Load(g, reader);
    private static void ReadJson(IGraph g, TextReader reader) => new JsonLdParser().Load(g.AsTripleStore(), reader);
    private static void ReadXml(IGraph g, TextReader reader) => new RdfXmlParser().Load(g, reader);
    private static void ReadRj(IGraph g, TextReader reader) => new RdfJsonParser().Load(g, reader);

    private static void WriteTurtle(IGraph g, TextWriter writer) => new CompressingTurtleWriter().Save(g, writer);
    private static void WriteNt(IGraph g, TextWriter writer) => new NTriplesWriter().Save(g, writer);
    private static void WriteJson(IGraph g, TextWriter writer) => new JsonLdWriter().Save(g.AsTripleStore(), writer);
    private static void WriteXml(IGraph g, TextWriter writer) => new RdfXmlWriter(WriterCompressionLevel.High, false).Save(g, writer);
    private static void WriteRj(IGraph g, TextWriter writer) => new RdfJsonWriter().Save(g, writer);
    private static void WriteCsv(IGraph g, TextWriter writer)
    {
        var results = (SparqlResultSet)g.ExecuteQuery($$"""
                PREFIX sh: <{{VDS.RDF.Shacl.Vocabulary.BaseUri}}>

                SELECT ?focusNode ?resultPath ?value ?sourceShape ?sourceConstraint ?sourceConstraintComponent ?resultSeverity ?resultMessage
                WHERE {
                    ?result
                        a sh:ValidationResult ;
                        sh:focusNode ?focusNode ;
                        sh:sourceShape ?sourceShape ;
                        sh:sourceConstraintComponent ?sourceConstraintComponent ;
                        sh:resultSeverity ?resultSeverity ;
                    .

                    OPTIONAL { ?result sh:resultPath ?resultPath . }
                    OPTIONAL { ?result sh:value ?value . }
                    OPTIONAL { ?result sh:sourceConstraint ?sourceConstraint . }
                    OPTIONAL { ?result sh:resultMessage ?resultMessage . }
                }

                """);

        new SparqlCsvWriter().Save(results, writer);
    }
}
