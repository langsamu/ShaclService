using System.Linq;
using VDS.RDF;
using Shacl = VDS.RDF.Shacl.Vocabulary;

namespace ShaclService.Models;

public class Result(INode n, IGraph g) : GraphWrapperNode(n, g)
{
    public INode SourceConstraintComponent => Shacl.SourceConstraintComponent.ObjectsOf(this).Single();

    public INode FocusNode => Shacl.FocusNode.ObjectsOf(this).Single();

    public INode Path => Shacl.ResultPath.ObjectsOf(this).SingleOrDefault();

    public INode Severity => Shacl.ResultSeverity.ObjectsOf(this).Single();

    public INode Shape => Shacl.SourceShape.ObjectsOf(this).Single();

    public INode Value => Shacl.Value.ObjectsOf(this).SingleOrDefault();

    public INode Message => Shacl.ResultMessage.ObjectsOf(this).SingleOrDefault();

    public INode SourceConstraint => Shacl.SourceConstraint.ObjectsOf(this).SingleOrDefault();
}
