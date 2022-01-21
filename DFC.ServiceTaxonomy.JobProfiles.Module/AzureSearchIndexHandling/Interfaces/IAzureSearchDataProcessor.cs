using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Interfaces
{
    public interface IAzureSearchDataProcessor<T>
    {
        Task<T> ProcessContentContext(ContentContextBase context, string actionType);
    }
}
