namespace ShaclService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using VDS.RDF;
    using VDS.RDF.Shacl;

    public class DefaultController : ControllerBase
    {
        [HttpPost("")]
        public IGraph Default([FromBody] IGraph shaclGraph)
        {
            var shapesGraph = new ShapesGraph(shaclGraph);
            var report = shapesGraph.Validate(shaclGraph).Normalised;

            return report;
        }
    }
}