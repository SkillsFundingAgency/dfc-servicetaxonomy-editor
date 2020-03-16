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

        Task<IEnumerable<string>> NodeLabels();
        string PropertyName(string name);
        string IdPropertyName { get; }
        string? IdPropertyValue(dynamic graphSyncContent);

        IEnumerable<string> NodeLabels(string contentType);
        string RelationshipType(string destinationContentType);
    }
    #pragma warning restore S4136
}
