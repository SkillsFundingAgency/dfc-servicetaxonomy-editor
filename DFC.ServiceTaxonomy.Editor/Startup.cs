using System.Threading.Tasks;
using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DFC.ServiceTaxonomy.Editor
{
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
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //occupation preview not working -> check got latest code and need to recreate site

            // UseCsp has to come before UseOrchardCore for it to have any affect
            app
                .UseCsp(csp =>
                {
                    // https://github.com/juunas11/aspnetcore-security-headers

                    csp.ByDefaultAllow.FromSelf();

                    csp.AllowScripts
                        .FromSelf()
                        .AllowUnsafeInline()
                        .AllowUnsafeEval()
                        .From("code.jquery.com")
                        .From("cdn.jsdelivr.net")
                        .From("cdnjs.cloudflare.com");

                    csp.AllowStyles.FromSelf()
                        .AllowUnsafeInline()
                        .From("fonts.googleapis.com")
                        .From("code.jquery.com")
                        .From("cdn.jsdelivr.net");

                    csp.AllowImages.FromSelf()
                        .DataScheme();

                    //do we need fromself for all if bydefaultallow allows it?
                    csp.AllowFonts.FromSelf()
                        .From("fonts.gstatic.com");

                    csp.AllowConnections
                        .ToSelf();

                    // csp.OnSendingHeader = context =>
                    // {
                    //     //todo: need to add all ajax callbacks? :-o
                    //     context.ShouldNotSend = context.HttpContext.Request.Path.StartsWithSegments("/Contents/ContentItems/");
                    //     return Task.CompletedTask;
                    // };
                })
                .UseOrchardCore();
        }
    }
}
