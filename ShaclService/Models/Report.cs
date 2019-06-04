namespace ShaclService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
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

    public class Result : WrapperNode
    {
        public Result(INode n)
            : base(n)
        {
        }

        public INode SourceConstraintComponent => Shacl.SourceConstraintComponent.ObjectsOf(this).Single();

        public INode FocusNode => Shacl.FocusNode.ObjectsOf(this).Single();

        public INode Path => Shacl.ResultPath.ObjectsOf(this).SingleOrDefault();

        public INode Severity => Shacl.ResultSeverity.ObjectsOf(this).Single();

        public INode Shape => Shacl.SourceShape.ObjectsOf(this).Single();

        public INode Value => Shacl.Value.ObjectsOf(this).SingleOrDefault();

        public INode Message => Shacl.ResultMessage.ObjectsOf(this).SingleOrDefault();
    }
}
