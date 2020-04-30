using DFC.ServiceTaxonomy.Editor.MethodProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using OrchardCore.Modules;
using OrchardCore.Scripting;

namespace DFC.ServiceTaxonomy.Editor
{
    //[RequireFeatures("OrchardCore.Scripting")]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(options =>
                options.InstrumentationKey = Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddOrchardCms();

            //services.AddScripting();
            services.AddSingleton<IGlobalMethodProvider, ConfigMethodProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOrchardCore();
        }
    }
}
