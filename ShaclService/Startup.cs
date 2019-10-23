namespace ShaclService
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Net.Http.Headers;
    using Swashbuckle.AspNetCore.SwaggerUI;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(ConfigureCors);
            services.AddControllersWithViews(ConfigureMvc);
            services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
                        { ".css", "text/css" },
                        { ".js", "text/javascript" },
                    }),
                });

            app.UseRewriter(new RewriteOptions().AddRewrite("^openapi$", "swagger/index.html", false).AddRewrite("^(swagger|favicon)(.+)$", "swagger/$1$2", true));
            app.UseCors();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseSwaggerUI(Startup.ConfigureSwaggerUI);
        }

        private static void ConfigureMvc(MvcOptions mvc)
        {
            mvc.InputFormatters.Clear();
            foreach (var item in Configuration.MediaTypes.Where(m => m.Read != null))
            {
                mvc.InputFormatters.Add(new GraphInputFormatter(item.MediaType, item.Read));
            }

            // TODO: why?
            mvc.InputFormatters.Add(new GraphInputFormatter("*/*", (g, reader) => { }));

            foreach (var item in Configuration.MediaTypes.Reverse())
            {
                mvc.OutputFormatters.Insert(0, new GraphOutputFormatter(item.MediaType, item.Write));
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(item.Extension, item.MediaType);
                mvc.FormatterMappings.SetMediaTypeMappingForFormat(item.MediaType, item.MediaType);
            }
        }

        private static void ConfigureSwaggerUI(SwaggerUIOptions swaggerUI)
        {
            swaggerUI.DocumentTitle = "dotNetRDF SHACL OpenAPI";
            swaggerUI.SwaggerEndpoint("./openapi.json", "live");
            swaggerUI.DefaultModelsExpandDepth(-1);
            swaggerUI.DisplayRequestDuration();
            swaggerUI.InjectStylesheet("./openapi.css");
            swaggerUI.EnableDeepLinking();
            swaggerUI.InjectJavascript("./openapi.js");
        }

        private static void ConfigureCors(CorsOptions cors)
        {
            cors.AddDefaultPolicy(policy => policy
                .AllowAnyOrigin()
                .WithHeaders(HeaderNames.ContentType)
                .WithMethods(HttpMethods.Post));
        }
    }
}
