using OrchardCore.ContentManagement.Handlers;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Accordions.Models;

namespace DFC.ServiceTaxonomy.Accordions.Handlers
{
    public class AccordionPartHandler : ContentPartHandler<AccordionPart>
    {
        public override Task InitializingAsync(InitializingContentContext context, AccordionPart part)
        {
            return Task.CompletedTask;
        }
    }
}
