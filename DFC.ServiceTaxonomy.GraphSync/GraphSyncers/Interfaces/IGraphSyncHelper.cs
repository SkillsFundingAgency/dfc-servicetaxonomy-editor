using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Settings;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    // we group methods by whether they work off the set ContentType property, or pass in a contentType
    #pragma warning disable S4136
    public interface IGraphSyncHelper
    {
        /// <summary>
        /// The content type of the content item being synced.
        /// This must be set before calling any other method or property that doesn't itself accept contentType.
        /// </summary>
        string? ContentType { get; set; }

        GraphSyncPartSettings GraphSyncPartSettings { get; }

        //todo: version that returns string with :
        Task<IEnumerable<string>> NodeLabels();
        Task<string> PropertyName(string name);
        string IdPropertyName();
        Task<string> GenerateIdPropertyValue();

        string IdPropertyName(string contentType);
        Task<IEnumerable<string>> NodeLabels(string contentType);
        Task<string> RelationshipTypeDefault(string destinationContentType);

        object GetIdPropertyValue(dynamic graphSyncContent);
    }
    #pragma warning restore S4136
}
