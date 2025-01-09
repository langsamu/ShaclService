using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;

namespace ShaclService;

public static class Extensions
{
    private static readonly NamespaceMapper Mapper = new NamespaceMapper();

    static Extensions()
    {
        Mapper.AddNamespace("sh", UriFactory.Create(Vocabulary.BaseUri));
        Mapper.AddNamespace("xsd", UriFactory.Create(XmlSpecsHelper.NamespaceXmlSchema));
    }

    public static string AsQName(this INode n)
    {
        if (n is IUriNode uriNode)
        {
            if (Mapper.ReduceToQName(uriNode.Uri.AbsoluteUri, out var qname))
            {
                return qname;
            }
        }

        return n.ToString();
    }

    public static string AbsoluteContent(this IUrlHelper url, string contentPath)
    {
        var request = url.ActionContext.HttpContext.Request;
        return new Uri(new Uri(request.Scheme + "://" + request.Host.Value), url.Content(contentPath)).ToString();
    }

    public static GraphWrapperNode In(this INode node, IGraph graph) => new (node, graph);

    internal static IEnumerable<GraphWrapperNode> ObjectsOf(this INode predicate, GraphWrapperNode subject) =>
            from t in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
            select t.Object.In(subject.Graph);

    internal static IInMemoryQueryableStore AsTripleStore(this IGraph g)
    {
        var store = new TripleStore();
        store.Add(g);
        return store;
    }
}
