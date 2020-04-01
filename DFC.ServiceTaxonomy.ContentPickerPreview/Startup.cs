using System;
using DFC.ServiceTaxonomy.ContentPickerPreview.Controllers;
using DFC.ServiceTaxonomy.ContentPickerPreview.Drivers;
using DFC.ServiceTaxonomy.ContentPickerPreview.Handlers;
using DFC.ServiceTaxonomy.ContentPickerPreview.Models;
using DFC.ServiceTaxonomy.ContentPickerPreview.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Data.Migration;
using OrchardCore.Mvc.Core.Utilities;

namespace DFC.ServiceTaxonomy.ContentPickerPreview
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        static Startup()
        {
            //todo: what's this?
            // TemplateContext.GlobalMemberAccessStrategy.Register<TitlePartViewModel>();
            // TemplateContext.GlobalMemberAccessStrategy.Register<TitlePartSettingsViewModel>();
        }

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
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
            routes.MapAreaControllerRoute(
                name: "ContentPickerPreview",
                areaName: "DFC.ServiceTaxonomy.ContentPickerPreview",
                //todo:
                pattern: _adminOptions.AdminUrlPrefix + "/ContentFields/SearchContentItems",
                defaults: new { controller = typeof(ContentPickerPreviewAdminController).ControllerName(), action = nameof(ContentPickerPreviewAdminController.SearchContentItems) }
            );
        }
    }
    //todo: ContentLocalization?
}
