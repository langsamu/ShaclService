using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using ShaclService.Formatters;
using ShaclService.Models;
using System.Collections.Generic;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(cors =>
    cors.AddDefaultPolicy(policy => policy
        .AllowAnyOrigin()
        .WithHeaders(HeaderNames.ContentType)
        .WithMethods(HttpMethods.Post)));

builder.Services.AddControllersWithViews(mvc =>
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
});
builder.Services.Configure<IISServerOptions>(options => options.AllowSynchronousIO = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
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
app.MapControllers();
app.UseSwaggerUI(swagger =>
{
    swagger.DocumentTitle = "dotNetRDF SHACL OpenAPI";
    swagger.SwaggerEndpoint("/openapi.json", "live");
    swagger.DefaultModelsExpandDepth(-1);
    swagger.DisplayRequestDuration();
    swagger.InjectStylesheet("./openapi.css");
    swagger.EnableDeepLinking();
    swagger.InjectJavascript("./openapi.js");

});

app.Run();

public partial class Program { }