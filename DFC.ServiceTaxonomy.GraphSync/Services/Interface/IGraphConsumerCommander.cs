using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.GraphSync.Services.Interface
{
    public interface IGraphConsumerCommander
    {
        Task<IEnumerable<GraphConsumer>> GetGraphConsumers();
    }
}
