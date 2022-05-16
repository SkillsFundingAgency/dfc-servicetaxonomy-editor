using Microsoft.Extensions.DependencyInjection;
using Fluid;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using DFC.ServiceTaxonomy.UnpublishLater.Drivers;
using DFC.ServiceTaxonomy.UnpublishLater.Handlers;
using DFC.ServiceTaxonomy.UnpublishLater.Indexes;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using DFC.ServiceTaxonomy.UnpublishLater.Services;
using DFC.ServiceTaxonomy.UnpublishLater.ViewModels;
using YesSql.Indexes;
using OrchardCore.AuditTrail.Services;

namespace DFC.ServiceTaxonomy.UnpublishLater
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<UnpublishLaterPartViewModel>();
            });

            services
                .AddContentPart<UnpublishLaterPart>()
                .UseDisplayDriver<UnpublishLaterPartDisplayDriver>()
                .AddHandler<UnpublishLaterPartHandler>();

            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<UnpublishLaterPartIndexProvider>();
            services.AddScoped<IScopedIndexProvider>(sp => sp.GetRequiredService<UnpublishLaterPartIndexProvider>());
            services.AddScoped<IContentHandler>(sp => sp.GetRequiredService<UnpublishLaterPartIndexProvider>());

            services.AddSingleton<IBackgroundTask, ScheduledUnpublishingBackgroundTask>();

            services.AddScoped<IAuditTrailEventHandler, UnpublishLaterAuditTrailEventHandler>();

        }
    }
}
