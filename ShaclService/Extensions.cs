namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Shacl;

    public static class Extensions
    {
        private static readonly NamespaceMapper mapper = new NamespaceMapper();

        static Extensions()
        {
            mapper.AddNamespace("sh", UriFactory.Create(Vocabulary.BaseUri));
            mapper.AddNamespace("xsd", UriFactory.Create(XmlSpecsHelper.NamespaceXmlSchema));
        }

        internal static IEnumerable<INode> ObjectsOf(this INode predicate, INode subject)
        {
            return
                from t in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
                select t.Object;
        }

        public static string AsQName(this INode n)
        {
            if (n is IUriNode uriNode)
            {
                if (mapper.ReduceToQName(uriNode.Uri.AbsoluteUri, out var qname))
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

        internal static IInMemoryQueryableStore AsTripleStore(this IGraph g)
        {
            var store = new TripleStore();
            store.Add(g);
            return store;
        }

        internal static string AsMediaType(this StringValues values)
        {
            return new MediaTypeHeaderValue((string)values).MediaType.Value;
        }
    }
}
