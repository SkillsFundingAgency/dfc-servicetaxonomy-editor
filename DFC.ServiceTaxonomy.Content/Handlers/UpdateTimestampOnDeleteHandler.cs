using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;

namespace DFC.ServiceTaxonomy.Content.Handlers
{
    public class UpdateTimestampOnDeleteHandler : ContentHandlerBase
    {
        private readonly IClock _clock;

        public UpdateTimestampOnDeleteHandler(IClock clock)
        {
            _clock = clock;
        }

        public override Task RemovingAsync(RemoveContentContext context)
        {
            context.ContentItem.ModifiedUtc = _clock.UtcNow;
            return Task.CompletedTask;
        }
    }
}
