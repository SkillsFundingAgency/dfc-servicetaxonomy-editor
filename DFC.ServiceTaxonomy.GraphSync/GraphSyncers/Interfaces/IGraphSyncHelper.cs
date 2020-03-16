using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces
{
    public interface IGraphSyncHelper
    {
        /// <summary>
        /// The content type of the content item being synced.
        /// This must be set before calling any other method or property.
        /// </summary>
        string? ContentType { get; set; }

        IEnumerable<string> NodeLabels { get; }
        string PropertyName(string name);
        string IdPropertyName { get; }
        string? IdPropertyValue(dynamic graphSyncContent);
    }
}
