using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Interfaces
{
    public interface IAzureSearchDataProcessor
    {
        Task ProcessContentContext(ContentContextBase context, string actionType);
    }
}
