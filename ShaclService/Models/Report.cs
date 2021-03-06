﻿namespace ShaclService
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;
    using Shacl = VDS.RDF.Shacl.Vocabulary;

    public class Report : WrapperNode
    {
        private Report(INode n)
            : base(n)
        {
        }

        public bool Conforms => Shacl.Conforms.ObjectsOf(this).Single().AsValuedNode().AsBoolean();

        public IEnumerable<Result> Results => Shacl.Result.ObjectsOf(this).Select(r => new Result(r));

        internal static Report Parse(IGraph g) =>
            new Report(g.GetTriplesWithPredicateObject(g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), Shacl.ValidationReport).Single().Subject);
    }
}
