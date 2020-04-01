using System;
using DFC.ServiceTaxonomy.ContentPickerPreview.Drivers;
using DFC.ServiceTaxonomy.ContentPickerPreview.Handlers;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;

namespace DFC.ServiceTaxonomy.ContentPickerPreview
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            //todo: what's this?
            // TemplateContext.GlobalMemberAccessStrategy.Register<TitlePartViewModel>();
            // TemplateContext.GlobalMemberAccessStrategy.Register<TitlePartSettingsViewModel>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            // Title Part
            services.AddContentPart<ContentPickerPreviewPart>()
                .UseDisplayDriver<ContentPickerPreviewPartDisplay>()
                .AddHandler<ContentPickerPreviewPartHandler>();

            // services.AddScoped<IContentPartIndexHandler, ContentPickerPreviewPartIndexHandler>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, ContentPickerPreviewPartSettingsDisplayDriver>();

            services.AddScoped<IDataMigration, Migrations>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
