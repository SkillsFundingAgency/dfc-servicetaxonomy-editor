using System.Threading.Tasks;
using OrchardCore.Media.Core.Events;
using OrchardCore.Media.Events;

namespace DFC.ServiceTaxonomy.Media.Events
{
    public class MediaBlobStoreEventHandler : MediaEventHandlerBase
    {
        public override Task MediaDeletedFileAsync(MediaDeletedContext context)
        {
            return Task.CompletedTask;
        }
    }
}
