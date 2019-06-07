namespace ShaclService
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.DependencyInjection;
    using Swashbuckle.AspNetCore.SwaggerUI;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(ConfigureMvc);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(
                new StaticFileOptions
                {
                    ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
                    {
                        { ".ttl", "text/turtle" },
                        { ".json", "application/json" },
                    }),
                });

            app.UseRewriter(new RewriteOptions().AddRewrite("^openapi.html$", "swagger/index.html", false).AddRewrite("^(swagger|favicon)(.+)$", "swagger/$1$2", true));
            app.UseMvc();
            app.UseSwaggerUI(Startup.ConfigureSwaggerUI);
        }

        private static void ConfigureMvc(MvcOptions mvc)
        {
            mvc.InputFormatters.Clear();
            foreach (var item in Configuration.MediaTypes.Where(m => m.Read != null))
            {
                mvc.InputFormatters.Add(new GraphInputFormatter(item.MediaType, item.Read));
            }

            mvc.OutputFormatters.Clear();
            foreach (var item in Configuration.MediaTypes)
            {
                mvc.OutputFormatters.Add(new GraphOutputFormatter(item.MediaType, item.Write));
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(item.Extension, item.MediaType);
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(item.MediaType, item.MediaType);
            }
        }

        private static void ConfigureSwaggerUI(SwaggerUIOptions swaggerUI)
        {
            swaggerUI.DocumentTitle = "dotNetRDF SHACL validator service";
            swaggerUI.SwaggerEndpoint("./openapi.json", "live");
        }
    }
}
