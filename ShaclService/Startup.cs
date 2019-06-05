namespace ShaclService
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(ConfigureMvc);
        }

        private static void ConfigureMvc(MvcOptions mvc)
        {
            mvc.InputFormatters.Clear();
            foreach (var item in Configuration.MediaTypes)
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
                        {".ttl", "text/turtle" }
                    })
                });

            app.UseMvc();
        }
    }
}
