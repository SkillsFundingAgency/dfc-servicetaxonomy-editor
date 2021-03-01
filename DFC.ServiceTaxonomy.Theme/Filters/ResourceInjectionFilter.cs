using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.Theme.Filters
{
    public class ResourceInjectionFilter : IAsyncResultFilter
    {
        private readonly IResourceManager _resourceManager;

        public ResourceInjectionFilter(IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is PartialViewResult)
            {
                await next();
                return;
            }

            _resourceManager
                .RegisterResource("script", "ncs")
                .AtFoot()
                .SetDependencies("jQuery");

            //todo: only required when page has trumbowyg editor
            _resourceManager
                .RegisterResource("script", "StaxTheme-trumbowyg-gds")
                .AtFoot()
                .SetDependencies("trumbowyg");    // trumbowyg-plugins?

            //todo: only required when page has trumbowyg editor
            _resourceManager
                .RegisterResource("stylesheet", "StaxTheme-trumbowyg-scoped-govuk-frontend")
                .AtHead();

            _resourceManager
                .RegisterResource("stylesheet", "StaxTheme-unpublish-later")
                .AtHead();

            //todo: only required for dashboard
            _resourceManager
                .RegisterResource("stylesheet", "StaxTheme-content-approval")
                .AtHead();

            await next();
        }
    }
}
