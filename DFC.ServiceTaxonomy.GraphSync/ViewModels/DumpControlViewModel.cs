using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Services;

namespace DFC.ServiceTaxonomy.GraphSync.ViewModels
{
    public class DumpControlViewModel
    {
        public IEnumerable<GraphConsumer>? GraphConsumers { get; set; }
    }
}
