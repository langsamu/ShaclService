namespace ShaclService
{
    using Microsoft.AspNetCore.Mvc;
    using System.ComponentModel.DataAnnotations;
    using VDS.RDF;
    using VDS.RDF.Shacl;

    public class ValidateController : Controller
    {
        [Route("validate")]
        [HttpGet]
        public IActionResult Default() =>
            View();

        [Route("validate/report")]
        [HttpPost]
        [HttpGet]
        public IActionResult ValidationReport(Parameters parameters) =>
            View(Report.Parse(ValidateRaw(parameters)));

        [Route("validate/raw")]
        [HttpGet]
        [HttpPost]
        public IGraph ValidateRaw(Parameters parameters) =>
            Validate(parameters.DataGraph, parameters.ShapesGraph);

        [HttpPost("validate/body")]
        public IGraph ValidateBody([FromBody] IGraph g) =>
            Validate(g, g);

        private static IGraph Validate(IGraph data, IGraph shapes) =>
            new ShapesGraph(shapes).Validate(data).Normalised;
    }
}