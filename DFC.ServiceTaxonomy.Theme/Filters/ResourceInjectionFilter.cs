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
            //todo: which plugins do we want to add
            //todo: what gds specific plugins do we want to create?
            //todo: front-end seems to eschewing gds styles and using its own
            _resourceManager
                .RegisterResource("stylesheet", "StaxTheme-trumbowyg-scoped-govuk-frontend")
                .AtHead();

            await next();
        }
    }
}
