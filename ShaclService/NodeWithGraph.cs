using VDS.RDF;

namespace ShaclService;

public class NodeWithGraph(INode node, IGraph graph) : WrapperNode(node)
{
    public IGraph Graph => graph;

    public INode Original => this.Node;
}
