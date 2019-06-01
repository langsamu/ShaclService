namespace ShaclService
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(ConfigureMvc);
        }

        private static void ConfigureMvc(MvcOptions mvc)
        {
            mvc.InputFormatters.Add(new GraphInputFormatter());
            mvc.OutputFormatters.Clear();
            mvc.OutputFormatters.Add(new GraphOutputFormatter());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
