using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace DFC.ServiceTaxonomy.Editor.Security
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app,IConfiguration configuration)
        {
            app.Use(async (context, next) =>
                {
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

                    if (!string.IsNullOrEmpty(context.Session?.Id))
                    {
                        context.Response.Headers.Add("X-WAF-SessionId", context.Session.Id);
                    }

                    await next();
                })
                // see https://github.com/juunas11/aspnetcore-security-headers
                // UseCsp has to come before UseOrchardCore for it to have any affect
                .UseCsp(csp =>
                {
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
                        .From("cdn.jsdelivr.net")
                        .From("cdnjs.cloudflare.com");

                    csp.AllowImages.FromSelf()
                        .DataScheme();

                    csp.AllowFonts.FromSelf()
                        .From("fonts.gstatic.com")
                        .From("cdn.jsdelivr.net");

                    csp.AllowConnections
                        .ToSelf();

                    csp.AllowImages
                       .FromSelf()
                       .From(configuration.GetValue<string>(Constants.Common.DigitalAssetsCdnKey));

                    csp.AllowFrames
                        .FromSelf()
                        .From("www.youtube-nocookie.com");
                });

            return app;
        }
    }
}
