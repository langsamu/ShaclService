namespace ShaclService
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using VDS.RDF;
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
                var g = new Graph();
                g.LoadFromString("<urn:s> <urn:p> <urn:o> .");
                await context.Response.WriteAsync(StringWriter.Write(g, new CompressingTurtleWriter()));
            });
        }
    }
}
