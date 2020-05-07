using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Events.Models;

namespace DFC.ServiceTaxonomy.Events.Services.Interfaces
{
    public interface IEventGridContentClient
    {
        Task Publish(ContentEvent contentEvent, CancellationToken cancellationToken = default);
        Task Publish(IEnumerable<ContentEvent> contentEvents, CancellationToken cancellationToken = default);
    }
}
