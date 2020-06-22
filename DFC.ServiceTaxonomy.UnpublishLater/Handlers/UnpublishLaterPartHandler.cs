using System.Threading.Tasks;
using DFC.ServiceTaxonomy.UnpublishLater.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.UnpublishLater.Handlers
{
    public class UnpublishLaterPartHandler : ContentPartHandler<UnpublishLaterPart>
    {
        public override Task PublishedAsync(PublishContentContext context, UnpublishLaterPart part)
        {
            part.ScheduledUnpublishUtc = null;
            part.Apply();

            return Task.CompletedTask;
        }
    }
}
