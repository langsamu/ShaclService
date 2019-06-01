namespace ShaclService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.IO;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Shacl;
    using VDS.RDF.Writing;

    public class DefaultController : ControllerBase
    {
        [HttpPost("")]
        public void Default()
        {
            var shaclGraph = new Graph();

            using (var reader = new StreamReader(Request.Body))
            {
                new TurtleParser().Load(shaclGraph, reader);
            }

            var shapesGraph = new ShapesGraph(shaclGraph);
            var report = shapesGraph.Validate(shaclGraph).Normalised;

            using (var writer = new StreamWriter(Response.Body))
            {
                new CompressingTurtleWriter().Save(report, writer);
            }
        }
    }
}