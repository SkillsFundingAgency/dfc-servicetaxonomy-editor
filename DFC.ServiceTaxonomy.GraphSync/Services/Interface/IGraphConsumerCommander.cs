using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IGraphConsumerCommander
    {
        IEnumerable<GraphConsumer> GraphConsumers { get; }
        // Task<IEnumerable<GraphConsumer>> GetGraphConsumers();
    }
}
