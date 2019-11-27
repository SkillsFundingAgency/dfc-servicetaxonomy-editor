using System.Collections.Generic;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Editor.Module.Services
{
    public interface INeoGraphDatabase
    {
        Task MergeNode(string nodeLabel, IDictionary<string,object> propertyMap, string idPropertyName = "uri");

        Task MergeRelationships(string sourceNodeLabel, string sourceIdPropertyName, string sourceIdPropertyValue,
            IDictionary<(string destNodeLabel, string destIdPropertyName, string relationshipType), IEnumerable<string>>
                relationships);
    }
}