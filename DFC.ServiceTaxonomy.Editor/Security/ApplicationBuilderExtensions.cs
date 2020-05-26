using Joonasw.AspNetCore.SecurityHeaders;
using Microsoft.AspNetCore.Builder;

namespace DFC.ServiceTaxonomy.Editor.Security
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
                {
                    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
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
                        .From("cdn.jsdelivr.net");

                    csp.AllowImages.FromSelf()
                        .DataScheme();

                    csp.AllowFonts.FromSelf()
                        .From("fonts.gstatic.com");

                    csp.AllowConnections
                        .ToSelf();
                });

            return app;
        }
    }
}
