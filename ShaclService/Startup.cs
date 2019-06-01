namespace ShaclService
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using System.IO;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Shacl;
    using VDS.RDF.Writing;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var shaclGraph = new Graph();

                using (var reader = new StreamReader(context.Request.Body))
                {
                    new TurtleParser().Load(shaclGraph, reader);
                }

                var shapesGraph = new ShapesGraph(shaclGraph);
                var report = shapesGraph.Validate(shaclGraph).Normalised;

                using (var writer = new StreamWriter(context.Response.Body))
                {
                    new CompressingTurtleWriter().Save(report, writer);
                }
            });
        }
    }
}
