namespace ShaclService
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;

    public static class Extensions
    {
        internal static IEnumerable<INode> ObjectsOf(this INode predicate, INode subject)
        {
            return
                from t in subject.Graph.GetTriplesWithSubjectPredicate(subject, predicate)
                select t.Object;
        }
    }
}
