using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.ResourceManagement;

namespace DFC.ServiceTaxonomy.PageLocation.Filters
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
                .RegisterResource("script", "pagelocations")
                .AtFoot()
                .SetDependencies("jQuery");

            await next();
        }
    }
}
