namespace DFC.ServiceTaxonomy.Events.Services.Interfaces
{
    public interface IEventGridContentRestHttpClientFactory
    {
        IRestHttpClient CreateClient(string contentType);
    }
}
