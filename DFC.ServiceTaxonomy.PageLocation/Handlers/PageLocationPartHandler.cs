using System.Threading.Tasks;
using DFC.ServiceTaxonomy.PageLocation.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.PageLocation.Handlers
{
    public class PageLocationPartHandler : ContentPartHandler<PageLocationPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, PageLocationPart part)
        {
            part.Apply();
            return Task.CompletedTask;
        }
    }
}
