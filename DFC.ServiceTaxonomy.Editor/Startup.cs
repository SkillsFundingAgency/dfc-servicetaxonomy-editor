using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace DFC.ServiceTaxonomy.Editor
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrchardCms();
            // services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseOrchardCore();

            // app.UseRouting();
            // app.UseEndpoints(ep => ep.MapControllers());

            // builder => builder.UseMvc(routes => routes.MapRoute(
            //     "Visualise", "{controller=Visualise}/{action=Index}/{id?}")));
        }

       //  public void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
       //  {
       //      routes.MapAreaControllerRoute(
       //          name: "Visualise",
       //          areaName: "DFC.ServiceTaxonomy.Editor",
       //          pattern: "Visualise/Index",
       //          defaults: new { controller = "Visualise", action = "Index" }
       //      );
       // }
    }
}
