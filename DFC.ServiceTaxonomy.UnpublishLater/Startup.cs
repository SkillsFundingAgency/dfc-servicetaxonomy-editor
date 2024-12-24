using DFC.ServiceTaxonomy.UnpublishLater.Drivers;
using DFC.ServiceTaxonomy.UnpublishLater.Handlers;
using DFC.ServiceTaxonomy.UnpublishLater.Indexes;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using DFC.ServiceTaxonomy.UnpublishLater.Services;
using DFC.ServiceTaxonomy.UnpublishLater.ViewModels;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace DFC.ServiceTaxonomy.UnpublishLater
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddContentPart<UnpublishLaterPart>()
                .UseDisplayDriver<UnpublishLaterPartDisplayDriver>()
                .AddHandler<UnpublishLaterPartHandler>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddSingleton<IIndexProvider, UnpublishLaterPartIndexProvider>();

            services.AddSingleton<IBackgroundTask, ScheduledUnpublishingBackgroundTask>();

            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<UnpublishLaterPartViewModel>();
            });
        }
    }
}
