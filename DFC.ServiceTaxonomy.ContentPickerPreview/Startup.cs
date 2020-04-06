using System;
using DFC.ServiceTaxonomy.ContentPickerPreview.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.ContentPickerPreview
{
    //options for preview picker:
    // derive from current field?
    // use real field and tweak in ui using js?

    //custom part
    //reuse contentpicker field
    //new render field
    //bit of js?

    //can we use a widget within the admin section?

    //todo: if get copy of vue-multiselect-wrapper working for poc, use original script and listen for vue-multiselect-created event and @@input to vue-multiselect?

    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        static Startup()
        {
            // these are needed to be able to access the view models  in display shapes rendered by the liquid markup engine
            // TemplateContext.GlobalMemberAccessStrategy.Register<TitlePartViewModel>();
            // TemplateContext.GlobalMemberAccessStrategy.Register<TitlePartSettingsViewModel>();
        }

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IContentPickerResultProvider, PreviewContentPickerResultProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseStaticFiles();
        }
    }
    //todo: ContentLocalization?
}
