using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;


namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces
{
    internal interface IDataEventProcessor
    {
        Task ProcessContentContext(ContentContextBase context, string actionType);
    }
}
