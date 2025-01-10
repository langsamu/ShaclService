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
using ShaclService.Formatters;
using ShaclService.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Collections.Generic;
using System.Linq;

namespace ShaclService;

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

        app.UseRewriter(new RewriteOptions().AddRewrite("^openapi$", "swagger/index.html", true).AddRewrite("^(swagger|favicon)(.+)$", "swagger/$1$2", true));
        app.UseCors();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
        app.UseSwaggerUI(Startup.ConfigureSwaggerUi);
    }

    private static void ConfigureMvc(MvcOptions mvc)
    {
        mvc.InputFormatters.Clear();
        foreach (var item in Configuration.MediaTypes.Where(m => m.Read is not null))
        {
            mvc.InputFormatters.Add(new GraphInputFormatter(item.MediaType, item.Read));
        }

        // TODO: why?
        mvc.InputFormatters.Add(new GraphInputFormatter("*/*", (_, _) => { }));

        foreach (var item in Configuration.MediaTypes.Reverse())
        {
            mvc.OutputFormatters.Insert(0, new GraphOutputFormatter(item.MediaType, item.Write));
            mvc.FormatterMappings.SetMediaTypeMappingForFormat(item.Extension, item.MediaType);
            mvc.FormatterMappings.SetMediaTypeMappingForFormat(item.MediaType, item.MediaType);
        }
    }

    private static void ConfigureSwaggerUi(SwaggerUIOptions swaggerUi)
    {
        swaggerUi.DocumentTitle = "dotNetRDF SHACL OpenAPI";
        swaggerUi.SwaggerEndpoint("/openapi.json", "live");
        swaggerUi.DefaultModelsExpandDepth(-1);
        swaggerUi.DisplayRequestDuration();
        swaggerUi.InjectStylesheet("./openapi.css");
        swaggerUi.EnableDeepLinking();
        swaggerUi.InjectJavascript("./openapi.js");
    }

    private static void ConfigureCors(CorsOptions cors) =>
        cors.AddDefaultPolicy(policy => policy
            .AllowAnyOrigin()
            .WithHeaders(HeaderNames.ContentType)
            .WithMethods(HttpMethods.Post));
}
