using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.ServiceBusHandling.Interfaces
{
    public interface IMessageConverter<out T>
    {
        T ConvertFrom(ContentItem contentItem);
    }
}
