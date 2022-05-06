using System.Threading.Tasks;

using OrchardCore.ContentManagement.Handlers;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.AzureSearch.Interfaces
{
    public interface IAzureSearchDataProcessor
    {
        Task ProcessContentContext(ContentContextBase context, string actionType);
    }
}
