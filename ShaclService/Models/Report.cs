﻿namespace ShaclService
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Nodes;
    using VDS.RDF.Parsing;
    using Shacl = VDS.RDF.Shacl.Vocabulary;

    public class Report : NodeWithGraph
    {
        private Report(INode n, IGraph g)
            : base(n, g)
        {
        }

        public bool Conforms => Shacl.Conforms.ObjectsOf(this).Single().AsValuedNode().AsBoolean();

        public IEnumerable<Result> Results => Shacl.Result.ObjectsOf(this).Select(r => new Result(r, this.Graph));

        internal static Report Parse(IGraph g) =>
            new Report(g.GetTriplesWithPredicateObject(g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType)), Shacl.ValidationReport).Single().Subject, g);
    }
}
