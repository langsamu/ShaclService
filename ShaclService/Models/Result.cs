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

    public class Result : NodeWithGraph
    {
        public Result(INode n, IGraph g)
            : base(n, g)
        {
        }

        public INode SourceConstraintComponent => Shacl.SourceConstraintComponent.ObjectsOf(this).Single();

        public INode FocusNode => Shacl.FocusNode.ObjectsOf(this).Single();

        public INode Path => Shacl.ResultPath.ObjectsOf(this).SingleOrDefault();

        public INode Severity => Shacl.ResultSeverity.ObjectsOf(this).Single();

        public INode Shape => Shacl.SourceShape.ObjectsOf(this).Single();

        public INode Value => Shacl.Value.ObjectsOf(this).SingleOrDefault();

        public INode Message => Shacl.ResultMessage.ObjectsOf(this).SingleOrDefault();

        public INode SourceConstraint => Shacl.SourceConstraint.ObjectsOf(this).SingleOrDefault();
    }
}
