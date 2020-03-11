using Microsoft.Extensions.Caching.Memory;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Cache;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class OrchardCoreContentDefinitionManager : ContentDefinitionManager, IOrchardCoreContentDefinitionManager
    {
        public OrchardCoreContentDefinitionManager(ISignal signal, IContentDefinitionStore contentDefinitionStore, IMemoryCache memoryCache) : base(signal, contentDefinitionStore, memoryCache)
        {
        }
    }
}
