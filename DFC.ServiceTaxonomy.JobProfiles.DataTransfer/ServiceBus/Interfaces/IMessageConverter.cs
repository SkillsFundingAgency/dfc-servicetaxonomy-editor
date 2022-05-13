using System.Threading.Tasks;

using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.ServiceBus.Interfaces
{
    public interface IMessageConverter<T>
    {
        Task<T> ConvertFromAsync(ContentItem contentItem);
    }
}
